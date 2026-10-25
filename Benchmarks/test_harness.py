# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
"""Non-vacuity/regression tests; actual framework behavior is checked by smoke runs."""
import json
import tempfile
from types import SimpleNamespace
import unittest
from pathlib import Path
from unittest.mock import patch
import xml.etree.ElementTree as ET

from generate import NO_RAW, SOURCES, arguments, generate, project, runner_project, suite, with_nunit_parallelism
from measurement import counts, measure, runner
from report import aot_columns, render
from run import prepare_project, publish_aot, rotated_order, run_interleaved


class HarnessTests(unittest.TestCase):
    def test_each_flavor_has_exact_fact_count_and_workload(self):
        for flavor in SOURCES:
            for raw in (False,) if flavor in NO_RAW else (False, True):
                with self.subTest(flavor=flavor, raw=raw):
                    source = suite(flavor, raw, 5, 2)
                    # The native Cratis runner finds should_* facts by convention, without an attribute.
                    attribute = "public void should_match_" if flavor in NO_RAW else "[Fact]" if flavor.startswith("xunit") else "[Test]"
                    self.assertEqual(10, source.count(attribute))
                    self.assertEqual(5, source.count("public class when_processing_"))
                    self.assertEqual(2, source.count("await Task.Yield()"))
                    self.assertEqual(2, source.count(" : given_context"))
                    self.assertIn("class given_context : given_seed", source)
                    self.assertIn("_description = _description.Trim();", source)
                    self.assertEqual(not raw, "ShouldEqual" in source)
                    self.assertEqual(not raw, "Cratis.Specifications" in source)

    def test_nunit_fixture_parallelism_is_explicit_and_nunit_only(self):
        attribute = "[assembly: NUnit.Framework.Parallelizable(NUnit.Framework.ParallelScope.Fixtures)]"
        for flavor in SOURCES:
            for raw in (False,) if flavor in NO_RAW else (False, True):
                with self.subTest(flavor=flavor, raw=raw):
                    plain = suite(flavor, raw, 5, 2)
                    self.assertEqual(plain, with_nunit_parallelism(flavor, plain, "none"))
                    source = with_nunit_parallelism(flavor, plain, "fixtures")
                    self.assertEqual(flavor.startswith("nunit"), attribute in source)
                    if flavor.startswith("nunit"):
                        self.assertLess(source.index(attribute), source.index("namespace BenchmarkSuite;"))
        self.assertEqual("none", arguments().parse_args([]).nunit_parallel)
        self.assertEqual("fixtures", arguments().parse_args(["--nunit-parallel", "fixtures"]).nunit_parallel)
        with patch.dict("os.environ", {"NUNIT_PARALLEL": "fixtures"}):
            self.assertEqual("fixtures", arguments().parse_args([]).nunit_parallel)

    def test_projects_are_single_tfm_non_packable_and_raw_is_independent(self):
        for flavor in SOURCES:
            for raw in (False,) if flavor in NO_RAW else (False, True):
                root = ET.fromstring(project(flavor, raw))
                self.assertEqual("net10.0", root.findtext(".//TargetFramework"))
                self.assertFalse(root.findtext(".//TargetFrameworks"))
                self.assertEqual("false", root.findtext(".//IsPackable"))
                # The native Cratis runner also references its source generator as an analyzer.
                self.assertEqual(0 if raw else 2 if flavor in NO_RAW else 1, len(root.findall(".//ProjectReference")))

    def test_work_parameter_adds_equal_context_work_to_every_flavor(self):
        for flavor in SOURCES:
            for raw in (False,) if flavor in NO_RAW else (False, True):
                with self.subTest(flavor=flavor, raw=raw):
                    source = suite(flavor, raw, 4, 2, work_us=500)
                    self.assertEqual(1, source.count("public static class BenchmarkWork"))
                    self.assertEqual(8, source.count("BenchmarkWork.Spin(125000)"))
                    self.assertEqual(4, source.count("await Task.Yield()"))
                    self.assertNotIn("BenchmarkWork", suite(flavor, raw, 4, 2))

    def test_generator_rejects_empty_unknown_or_duplicate_population(self):
        for flavors in ("", "unknown", "tunit,tunit"):
            args = arguments().parse_args(["--flavors", flavors])
            with self.assertRaises(ValueError):
                generate(args)

    def test_trx_accepts_only_exactly_expected_passes(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "report.trx"
            for total, passed, failed in ((10, 10, 0), (0, 0, 0), (9, 9, 0), (11, 11, 0), (10, 9, 1), (10, 9, 0)):
                path.write_text(f'<TestRun xmlns="urn:trx"><ResultSummary><Counters total="{total}" passed="{passed}" failed="{failed}"/></ResultSummary></TestRun>')
                if (total, passed, failed) == (10, 10, 0):
                    self.assertEqual(10, counts(path, 10))
                else:
                    with self.assertRaises(ValueError):
                        counts(path, 10)
            path.write_text("<TestRun/>")
            with self.assertRaises(ValueError):
                counts(path, 10)
            with self.assertRaises(FileNotFoundError):
                counts(Path(directory) / "missing.trx", 10)

    def test_xunit_xml_rejects_errors_skips_and_empty_assemblies(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "report.xml"
            for skipped, errors in ((0, 0), (1, 0), (0, 1)):
                path.write_text(f'<assemblies><assembly total="10" passed="10" failed="0" skipped="{skipped}" errors="{errors}"/></assemblies>')
                if skipped or errors:
                    with self.assertRaises(ValueError):
                        counts(path, 10)
                else:
                    self.assertEqual(10, counts(path, 10))
            path.write_text("<assemblies/>")
            with self.assertRaises(ValueError):
                counts(path, 10)

    def test_every_natural_runner_has_a_report_and_serial_setting(self):
        with tempfile.TemporaryDirectory() as directory:
            project_path = Path(directory) / "Benchmark.csproj"
            (project_path.parent / "bin/Release/net10.0").mkdir(parents=True)
            for flavor, route, setting in (("xunit-v2", "dotnet-test", "--logger"), ("xunit-v3", "executable", "-parallelMode"), ("xunit-v3-mtp", "executable", "--parallel"), ("xunit-v3-mtp", "dotnet-run", "--parallel"), ("nunit4", "dotnet-test", "NUnit.NumberOfTestWorkers=0"), ("tunit", "executable", "--maximum-parallel-tests"), ("tunit", "dotnet-run", "--maximum-parallel-tests"), ("cratis-runner", "executable", "--specs-parallel"), ("cratis-runner", "aot", "--specs-sequential-facts")):
                command, report = runner(project_path, flavor, route, True, Path(directory))
                self.assertIn(setting, command)
                self.assertIn(str(report) if route == "executable" and flavor == "xunit-v3" else str(Path(directory)), command)

    def test_aot_is_toggled_by_a_benchmark_specific_property_not_a_global_one(self):
        with tempfile.TemporaryDirectory() as directory, patch("run.measure", return_value={"exit": 0, "log": str(Path(directory) / "publish.log")}) as measure:
            (Path(directory) / "publish.log").write_text("")
            publish_aot(Path("Cratis/Benchmark.csproj"), Path("logs"))
        command = measure.call_args.args[0]
        self.assertIn("-p:BenchmarkAot=true", command)
        self.assertNotIn("-p:PublishAot=true", command)
        self.assertIn("BenchmarkAot", runner_project())

    def test_xunit_mtp_is_distinct_from_the_native_mtp_off_runner(self):
        mtp = project("xunit-v3-mtp", False)
        self.assertIn('Include="xunit.v3"', mtp)
        self.assertNotIn("mtp-off", mtp)
        self.assertIn('Include="Microsoft.Testing.Extensions.TrxReport"', mtp)
        self.assertIn("<UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>", mtp)
        self.assertNotIn("UseMicrosoftTestingPlatformRunner", project("xunit-v3", False))
        self.assertNotIn("Microsoft.NET.Test.Sdk", mtp)
        self.assertIn("<OutputType>Exe</OutputType>", mtp)
        for raw in (False, True):
            self.assertEqual(suite("xunit-v3", raw, 5, 2), suite("xunit-v3-mtp", raw, 5, 2))
        self.assertIn('Include="xunit.v3.mtp-off"', project("xunit-v3", False))

    def test_aot_publish_counts_unique_trim_and_aot_warnings(self):
        with tempfile.TemporaryDirectory() as directory:
            log = Path(directory) / "publish.log"
            log.write_text("file: warning IL2026: trim\nfile: warning IL2026: trim\nfile: warning IL3050: aot\nfile: warning CS1234: other\n")
            with patch("run.measure", return_value={"exit": 0, "seconds": 2, "log": str(log)}):
                self.assertEqual(2, publish_aot(Path("Cratis/Benchmark.csproj"), Path(directory))["aot_trim_warnings"])

    def test_time_wrapper_preserves_child_failure_and_records_rss(self):
        with tempfile.TemporaryDirectory() as directory:
            result = measure(["/bin/sh", "-c", "echo intentional-failure; exit 7"], Path(directory) / "measurement")
            self.assertEqual(7, result["exit"])
            self.assertGreater(result["seconds"], 0)
            self.assertIsNotNone(result["peak_rss_mib"])
            self.assertGreaterEqual(result["cpu_seconds"], 0)
            self.assertIn("intentional-failure", Path(result["log"]).read_text())

    def test_interleaving_rotates_every_variant_once_per_pass(self):
        rows = [{"id": (flavor, suite, mode, route)}
                for flavor in ("xunit-v3", "tunit") for suite in ("Cratis", "raw")
                for mode in ("default", "off") for route in ("cli", "executable")]
        self.assertEqual([], rotated_order([], 1))
        repetitions = []
        with patch("run.run_sample") as sample, patch("run.load", return_value=(1, 2, 3)):
            run_interleaved(rows, SimpleNamespace(r=len(rows)), Path("unused"), repetitions)
        calls = [(call.args[0], call.args[3]) for call in sample.call_args_list]
        for repetition in range(len(rows) + 1):
            offset = repetition % len(rows)
            expected = rows[offset:] + rows[:offset]
            self.assertEqual([(row, repetition) for row in expected],
                             calls[repetition * len(rows):(repetition + 1) * len(rows)])
        self.assertEqual(list(range(len(rows) + 1)), [p["repetition"] for p in repetitions])
        self.assertTrue(all(p["load_before"] == (1, 2, 3) and p["load_after"] == (1, 2, 3)
                            for p in repetitions))

    def test_preparation_builds_without_measuring_and_retains_all_modes(self):
        with tempfile.TemporaryDirectory() as directory, patch("run.build", return_value={}) as build, \
                patch("run.measure") as measure, patch("builtins.print"):
            rows = []
            args = SimpleNamespace(aot=False, parallel="both")
            for suite in ("Cratis", "raw"):
                item = {"flavor": "xunit-v3", "suite": suite, "project": f"{suite}/Benchmark.csproj"}
                prepare_project(item, args, Path(directory), rows)
            self.assertEqual(2, build.call_count)
            measure.assert_not_called()
            self.assertEqual(8, len(rows))
            self.assertEqual({(suite, mode, route) for suite in ("Cratis", "raw")
                              for mode in ("default", "off") for route in ("dotnet-test", "executable")},
                             {(row["suite"], row["parallel"], row["route"]) for row in rows})

    def test_failed_counts_invalidate_only_that_variant_without_retries(self):
        with tempfile.TemporaryDirectory() as directory, patch("run.runner", return_value=([], Path("unused"))), \
                patch("run.measure", side_effect=lambda *args: {"exit": 0, "seconds": 1, "cpu_seconds": 0.5, "peak_rss_mib": 20}), \
                patch("run.counts", side_effect=[ValueError("wrong count"), 10, 10, 10]) as count, \
                patch("run.load", return_value=(1, 2, 3)), patch("builtins.print"):
            rows = [{"flavor": "xunit-v3", "project": f"{name}/Benchmark.csproj",
                     "parallel": "default", "route": "executable", "samples": []}
                    for name in ("bad", "good")]
            for name in ("bad", "good"):
                (Path(directory) / name).mkdir()
            run_interleaved(rows, SimpleNamespace(n=5, f=2, r=2), Path(directory), [])
            self.assertFalse(rows[0]["warmup"]["valid"])
            self.assertEqual([], rows[0]["samples"])
            self.assertEqual([1, 2], [s["repetition"] for s in rows[1]["samples"]])
            self.assertTrue(all(s["valid"] for s in rows[1]["samples"]))
            self.assertEqual(4, count.call_count)
            self.assertTrue(all(call.args[1] == 10 for call in count.call_args_list))

    def test_cpu_time_is_user_plus_system_child_usage_delta(self):
        before = SimpleNamespace(ru_utime=10.25, ru_stime=4.5)
        after = SimpleNamespace(ru_utime=11.75, ru_stime=5.25)
        with tempfile.TemporaryDirectory() as directory, \
                patch("measurement.resource.getrusage", side_effect=[before, after]) as usage:
            result = measure(["/bin/sh", "-c", "exit 0"], Path(directory) / "measurement")
            self.assertEqual(2.25, result["cpu_seconds"])
            self.assertEqual(2, usage.call_count)
            self.assertEqual(usage.call_args_list[0], usage.call_args_list[1])

    def test_complete_samples_report_median_range_and_peak(self):
        with tempfile.TemporaryDirectory() as directory, patch("builtins.print"):
            row = {"flavor": "tunit", "suite": "Cratis", "parallel": "off", "route": "executable", "warmup": {"seconds": 99, "cpu_seconds": 99}, "samples": [{"valid": True, "seconds": s, "cpu_seconds": c, "peak_rss_mib": m} for s, c, m in ((2, 0.5, 30), (1, 0.9, 40), (3, 0.1, 20))]}
            data = {"n": 5, "f": 2, "r": 3, "host": "test", "platform": "test", "sdk": "test", "load_before": "test", "load_after": "test", "rows": [row]}
            render(data, Path(directory) / "result")
            self.assertTrue(row["valid"])
            self.assertEqual(0.5, row["median_cpu_seconds"])
            self.assertIn("CPU median (s)", (Path(directory) / "result.md").read_text())
            self.assertEqual(0.5, json.loads((Path(directory) / "result.json").read_text())["rows"][0]["median_cpu_seconds"])
            self.assertEqual((2, 1, 3, 40), (row["median_seconds"], row["min_seconds"], row["max_seconds"], row["peak_rss_mib"]))

    def test_load_averages_are_retained_in_markdown_and_json(self):
        with tempfile.TemporaryDirectory() as directory, patch("builtins.print"):
            passes = [{"repetition": r, "load_before": (1, 2, 3), "load_after": (4, 5, 6)}
                      for r in range(3)]
            data = {"n": 5, "f": 2, "r": 2, "host": "test", "platform": "test", "sdk": "test",
                    "load_before": (7, 8, 9), "load_after": (10, 11, 12), "repetitions": passes, "rows": []}
            render(data, Path(directory) / "result")
            markdown = (Path(directory) / "result.md").read_text()
            self.assertIn("Load before: `(7, 8, 9)`", markdown)
            self.assertIn("Load after: `(10, 11, 12)`", markdown)
            for label in ("Warm-up", "1", "2"):
                self.assertIn(f"| {label} | (1, 2, 3) | (4, 5, 6) |", markdown)
            saved = json.loads((Path(directory) / "result.json").read_text())
            self.assertEqual([7, 8, 9], saved["load_before"])
            self.assertEqual([10, 11, 12], saved["load_after"])
            self.assertEqual(3, len(saved["repetitions"]))
            self.assertTrue(all(p["load_before"] == [1, 2, 3] and p["load_after"] == [4, 5, 6]
                                for p in saved["repetitions"]))

    def test_partial_samples_never_become_a_valid_median(self):
        with tempfile.TemporaryDirectory() as directory, patch("builtins.print"):
            row = {"flavor": "xunit-v2", "suite": "Cratis", "parallel": "off", "route": "dotnet-test", "samples": [{"valid": True, "seconds": 1, "peak_rss_mib": 20}]}
            data = {"n": 5, "f": 2, "r": 2, "host": "test", "platform": "test", "sdk": "test", "load_before": "test", "load_after": "test", "rows": [row]}
            render(data, Path(directory) / "result")
            self.assertFalse(row["valid"])
            self.assertNotIn("median_seconds", row)


if __name__ == "__main__":
    unittest.main()
