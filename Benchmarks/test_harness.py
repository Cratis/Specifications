# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
"""Non-vacuity/regression tests; actual framework behavior is checked by smoke runs."""
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch
import xml.etree.ElementTree as ET

from generate import NO_RAW, SOURCES, arguments, generate, project, suite
from measurement import counts, measure, runner
from report import render


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
            for flavor, route, setting in (("xunit-v2", "dotnet-test", "--logger"), ("xunit-v3", "executable", "-parallelMode"), ("nunit4", "dotnet-test", "NUnit.NumberOfTestWorkers=0"), ("tunit", "executable", "--maximum-parallel-tests"), ("tunit", "dotnet-run", "--maximum-parallel-tests"), ("cratis-runner", "executable", "--specs-parallel"), ("cratis-runner", "aot", "--specs-sequential-facts")):
                command, report = runner(project_path, flavor, route, True, Path(directory))
                self.assertIn(setting, command)
                self.assertIn(str(report) if route == "executable" and flavor == "xunit-v3" else str(Path(directory)), command)

    def test_time_wrapper_preserves_child_failure_and_records_rss(self):
        with tempfile.TemporaryDirectory() as directory:
            result = measure(["/bin/sh", "-c", "echo intentional-failure; exit 7"], Path(directory) / "measurement")
            self.assertEqual(7, result["exit"])
            self.assertGreater(result["seconds"], 0)
            self.assertIsNotNone(result["peak_rss_mib"])
            self.assertIn("intentional-failure", Path(result["log"]).read_text())

    def test_complete_samples_report_median_range_and_peak(self):
        with tempfile.TemporaryDirectory() as directory, patch("builtins.print"):
            row = {"flavor": "tunit", "suite": "Cratis", "parallel": "off", "route": "executable", "warmup": {"seconds": 99}, "samples": [{"valid": True, "seconds": s, "peak_rss_mib": m} for s, m in ((2, 30), (1, 40), (3, 20))]}
            data = {"n": 5, "f": 2, "r": 3, "host": "test", "platform": "test", "sdk": "test", "load_before": "test", "load_after": "test", "rows": [row]}
            render(data, Path(directory) / "result")
            self.assertTrue(row["valid"])
            self.assertEqual((2, 1, 3, 40), (row["median_seconds"], row["min_seconds"], row["max_seconds"], row["peak_rss_mib"]))

    def test_partial_samples_never_become_a_valid_median(self):
        with tempfile.TemporaryDirectory() as directory, patch("builtins.print"):
            row = {"flavor": "xunit-v2", "suite": "Cratis", "parallel": "off", "route": "dotnet-test", "samples": [{"valid": True, "seconds": 1, "peak_rss_mib": 20}]}
            data = {"n": 5, "f": 2, "r": 2, "host": "test", "platform": "test", "sdk": "test", "load_before": "test", "load_after": "test", "rows": [row]}
            render(data, Path(directory) / "result")
            self.assertFalse(row["valid"])
            self.assertNotIn("median_seconds", row)


if __name__ == "__main__":
    unittest.main()
