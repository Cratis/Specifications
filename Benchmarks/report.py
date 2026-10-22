# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
"""Write complete raw measurements plus a compact comparison table."""
import json
import statistics


def render(data, destination):
    lines = ["# Specifications benchmark", "", f"N={data['n']}, F={data['f']}, R={data['r']}; one excluded warm-up pass; rotated, interleaved repetitions.",
             f"Host: {data['host']}; OS: {data['platform']}; SDK: {data['sdk']}.",
             f"Load before: `{data['load_before']}`", f"Load after: `{data['load_after']}`", "",
             "Shared/loaded machine: results include startup, discovery and reporting; not a causal ranking of framework overhead.", "",
             "CPU time (user + system) is the primary comparison for per-test overhead on a loaded machine; wall time includes scheduling delays.", "",
             "Clean means project Rebuild after untimed restore; dependencies/cache may already be warm. RSS is the maximum reported for the timed process tree (not summed concurrent memory).", "",
             "| Flavor | Suite | Parallel | Runner | Build clean / no-op (s) | CPU median (s) | Wall median [min, max] (s) | Peak RSS (MiB) | Tests | Status |",
             "|---|---|---|---|---|---|---|---|---|---|"]
    for row in data["rows"]:
        samples = row.get("samples", [])
        valid = [s for s in samples if s.get("valid")]
        complete = len(valid) == data["r"] and not row.get("error")
        row["valid"] = complete
        if complete:
            times = [s["seconds"] for s in valid]
            row["median_cpu_seconds"] = statistics.median(s["cpu_seconds"] for s in valid)
            cpu = f"{row['median_cpu_seconds']:.3f}"
            row["median_seconds"] = statistics.median(times)
            row["min_seconds"], row["max_seconds"] = min(times), max(times)
            row["peak_rss_mib"] = max(s["peak_rss_mib"] for s in valid)
            wall = f"{row['median_seconds']:.3f} [{min(times):.3f}, {max(times):.3f}]"
            memory = f"{row['peak_rss_mib']:.1f}"
        else:
            cpu, wall, memory = "—", "—", "—"
        builds = row.get("builds", {})
        build = " / ".join(f"{builds[k]['seconds']:.3f}" if k in builds else "—" for k in ("clean", "incremental"))
        status = "valid" if complete else "INVALID: " + row.get("error", "incomplete samples")
        lines.append(f"| {row['flavor']} | {row['suite']} | {row['parallel']} | {row['route']} | {build} | {cpu} | {wall} | {memory} | {data['n'] * data['f'] if complete else '—'} | {status} |")
    lines += ["", "Load averages (1, 5, 15 minutes) per pass:", "",
              "| Pass | Start | End |", "|---|---|---|"]
    for repetition in data.get("repetitions", []):
        label = "Warm-up" if repetition["repetition"] == 0 else str(repetition["repetition"])
        lines.append(f"| {label} | {repetition['load_before']} | {repetition['load_after']} |")
    destination.with_suffix(".md").write_text("\n".join(lines) + "\n")
    destination.with_suffix(".json").write_text(json.dumps(data, indent=2) + "\n")
    print("\n".join(lines), flush=True)
