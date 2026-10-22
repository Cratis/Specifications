# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
"""Runner-independent wall/RSS measurement and strict structured result validation."""
import platform
import re
import subprocess
import time
import xml.etree.ElementTree as ET


def measure(command, prefix):
    output = prefix.with_suffix(".log")
    memory = prefix.with_suffix(".time")
    flag = "-l" if platform.system() == "Darwin" else "-v"
    started = time.perf_counter()
    with output.open("w") as log:
        process = subprocess.run(["/usr/bin/time", flag, "-o", str(memory), *command], stdout=log, stderr=subprocess.STDOUT)
    seconds = time.perf_counter() - started
    stats = memory.read_text()
    match = re.search(r"(\d+)\s+maximum resident set size", stats) if flag == "-l" else re.search(r"Maximum resident set size \(kbytes\):\s*(\d+)", stats)
    rss = int(match[1]) / (1024 * 1024 if flag == "-l" else 1024) if match else None
    return {"command": command, "exit": process.returncode, "seconds": seconds, "peak_rss_mib": rss, "log": str(output)}


def counts(path, expected):
    root = ET.parse(path).getroot()
    if root.tag == "assemblies":
        assemblies = root.findall("assembly")
        if not assemblies:
            raise ValueError("xUnit report has no assemblies")
        total = sum(int(a.attrib["total"]) for a in assemblies)
        passed = sum(int(a.attrib["passed"]) for a in assemblies)
        failed = sum(int(a.attrib["failed"]) + int(a.attrib.get("errors", 0)) for a in assemblies)
        skipped = sum(int(a.attrib["skipped"]) for a in assemblies)
    else:
        counter = root.find(".//{*}Counters")
        if counter is None:
            raise ValueError("TRX report has no counters")
        total, passed, failed = (int(counter.attrib[k]) for k in ("total", "passed", "failed"))
        skipped = total - passed - failed
    if (total, passed, failed, skipped) != (expected, expected, 0, 0):
        raise ValueError(f"expected {expected} passing tests; total={total}, passed={passed}, failed={failed}, skipped={skipped}")
    return total


def runner(project, flavor, route, serial, result_dir):
    directory = project.parent
    binary = directory / "bin/Release/net10.0/Benchmark"
    output = binary.parent
    if flavor.startswith("xunit"):
        # Both the adapter and the standalone xUnit runner read this configuration.
        import json
        (output / "xunit.runner.json").write_text(json.dumps({"parallelizeTestCollections": False, "maxParallelThreads": 1} if serial else {}))
    if route == "dotnet-test":
        report = result_dir / "results.trx"
        command = ["dotnet", "test", str(project), "-c", "Release", "-f", "net10.0", "--no-build", "--no-restore", "--results-directory", str(result_dir), "--logger", "trx;LogFileName=results.trx"]
        if serial and flavor.startswith("nunit"):
            command += ["--", "NUnit.NumberOfTestWorkers=0"]
    elif flavor == "xunit-v3":
        report = result_dir / "results.xml"
        command = [str(binary), str(output / "xunit.runner.json"), "-xml", str(report), "-noLogo"]
        if serial:
            command += ["-parallelMode", "none"]
    else:
        report = result_dir / "results.trx"
        command = (["dotnet", "run", "--project", str(project), "-c", "Release", "-f", "net10.0", "--no-build", "--no-restore", "--"] if route == "dotnet-run" else [str(binary)])
        command += ["--report-trx", "--report-trx-filename", "results.trx", "--results-directory", str(result_dir)]
        if serial:
            command += ["--maximum-parallel-tests", "1"]
    return command, report
