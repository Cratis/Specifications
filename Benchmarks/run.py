#!/usr/bin/env python3
# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
"""Build generated suites, then measure all variants in rotated, interleaved passes."""
import datetime
import os
import platform
import socket
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

from generate import ROOT, arguments, generate, positive
from measurement import counts, measure, runner
from report import render


def load():
    return os.getloadavg()


def runtime_identifier():
    machine = platform.machine().lower()
    arch = "arm64" if machine in ("arm64", "aarch64") else "x64"
    return f"{'osx' if platform.system() == 'Darwin' else 'linux'}-{arch}"


def publish_aot(project, logs):
    # Native AOT publish of the native Cratis runner; timed like a clean build, measured separately.
    result = measure(["dotnet", "publish", str(project), "-c", "Release", "-f", "net10.0", "-r", runtime_identifier(),
                      "-p:BenchmarkAot=true", "-o", str(project.parent / "bin/aot")], logs / "aot-publish")
    if result["exit"]:
        raise RuntimeError(f"AOT publish failed: {result['log']}")
    return result


def build(project, logs):
    # Restore is intentionally outside the build timer (network is not compiler work).
    with (logs / "restore.log").open("w") as log:
        result = subprocess.run(["dotnet", "restore", str(project)], stdout=log, stderr=subprocess.STDOUT)
    if result.returncode:
        raise RuntimeError(f"restore failed: {logs / 'restore.log'}")
    builds = {}
    for name, extra in (("clean", ["-t:Rebuild"]), ("incremental", [])):
        builds[name] = measure(["dotnet", "build", str(project), "-c", "Release", "-f", "net10.0", "--no-restore", *extra], logs / name)
        if builds[name]["exit"] or builds[name]["peak_rss_mib"] is None:
            raise RuntimeError(f"{name} build failed: {builds[name]['log']}")
    return builds


def prepare_project(item, args, directory, rows):
    project, flavor = Path(item["project"]), item["flavor"]
    logs = directory / project.parent.name
    logs.mkdir(parents=True)
    print(f"Building {project.parent.name}", flush=True)
    try:
        builds = build(project, logs)
    except RuntimeError as error:
        rows.append({**item, "parallel": "—", "route": "build", "error": str(error)})
        return
    if flavor == "cratis-runner" and args.aot:
        try:
            builds["aot"] = publish_aot(project, logs)
        except RuntimeError as error:
            rows.append({**item, "parallel": "—", "route": "aot", "error": str(error)})
    routes = ["dotnet-run", "executable", *(["aot"] if "aot" in builds else [])] if flavor == "cratis-runner" else \
        ["dotnet-run", "executable"] if flavor == "tunit" else ["dotnet-test", "executable"] if flavor == "xunit-v3" else ["dotnet-test"]
    modes = ["default", "off"] if args.parallel == "both" else [args.parallel]
    for mode in modes:
        for route in routes:
            row = {**item, "parallel": mode, "route": route, "builds": builds, "samples": []}
            rows.append(row)


def rotated_order(rows, repetition):
    if not rows:
        return []
    offset = repetition % len(rows)
    return rows[offset:] + rows[:offset]


def run_sample(row, args, directory, repetition):
    project = Path(row["project"])
    mode, route = row["parallel"], row["route"]
    run_dir = directory / project.parent.name / f"{mode}-{route}-{repetition}"
    run_dir.mkdir()
    command, report = runner(project, row["flavor"], route, mode == "off", run_dir)
    print(f"{project.parent.name} {mode} {route} {'warm-up' if repetition == 0 else repetition}", flush=True)
    sample = measure(command, run_dir / "measurement")
    sample["repetition"] = repetition
    try:
        if sample["exit"]:
            raise ValueError(f"runner exit {sample['exit']}; {sample['log']}")
        if sample["peak_rss_mib"] is None:
            raise ValueError("missing peak RSS")
        if sample["cpu_seconds"] is None:
            raise ValueError("missing CPU time")
        sample["count"] = counts(report, args.n * args.f)
        sample["valid"] = True
    except (ValueError, OSError, KeyError, ET.ParseError) as error:
        sample["valid"] = False
        row["error"] = str(error)
    if repetition == 0:
        row["warmup"] = sample
    else:
        row["samples"].append(sample)


def run_interleaved(rows, args, directory, repetitions):
    for repetition in range(args.r + 1):
        record = {"repetition": repetition, "load_before": load()}
        repetitions.append(record)
        try:
            for row in rotated_order(rows, repetition):
                if not row.get("error"):
                    run_sample(row, args, directory, repetition)
        finally:
            record["load_after"] = load()


def main():
    parser = arguments()
    parser.add_argument("--r", type=positive, default=os.getenv("R", "5"))
    parser.add_argument("--parallel", choices=["default", "off", "both"], default=os.getenv("PARALLEL", "both"))
    parser.add_argument("--aot", action="store_true", default=os.getenv("AOT") == "1",
                        help="also publish the cratis-runner suite with Native AOT and measure the native executable")
    args = parser.parse_args()
    projects = generate(args)
    host = socket.gethostname().split(".")[0]
    stamp = datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
    destination = ROOT / "results" / f"{stamp}-{host}"
    directory = destination.parent / (destination.name + "-logs")
    directory.mkdir(parents=True)
    data = {"n": args.n, "f": args.f, "r": args.r, "host": host, "platform": platform.platform(),
            "sdk": subprocess.check_output(["dotnet", "--version"], text=True).strip(),
            "work_us": args.work_us, "load_before": load(), "repetitions": [], "rows": []}
    try:
        for item in projects:
            prepare_project(item, args, directory, data["rows"])
        run_interleaved(data["rows"], args, directory, data["repetitions"])
    finally:
        data["load_after"] = load()
        render(data, destination)
        print(f"Results: {destination}.md", flush=True)
    return 0 if data["rows"] and all(row["valid"] for row in data["rows"]) else 1


if __name__ == "__main__":
    raise SystemExit(main())
