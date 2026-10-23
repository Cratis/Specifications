// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;

#if MICRO
if (args is ["--analyzer-telemetry", var path])
{
    var workload = new Cratis.Specifications.Benchmarks.CompilationBenchmarks();
    workload.Setup();
    await workload.WriteTelemetry(path);
    return 0;
}
#endif
var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args).ToArray();
return summaries.Length > 0 && summaries.All(summary => !summary.HasCriticalValidationErrors && summary.Reports.Any() && summary.Reports.All(report => report.Success)) ? 0 : 1;
