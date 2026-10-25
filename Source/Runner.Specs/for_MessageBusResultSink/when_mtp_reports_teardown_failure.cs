// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Xml.Linq;

namespace Cratis.Specifications.Runner.for_MessageBusResultSink;

public class when_mtp_reports_teardown_failure : Specification
{
    readonly string directory = Path.Combine(Path.GetTempPath(), $"specifications-teardown-{Guid.NewGuid():N}");
    int exit_code;
    XDocument trx;

    async Task Because()
    {
        Directory.CreateDirectory(directory);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                ArgumentList =
                {
                    typeof(failing_fact_and_teardown).Assembly.Location,
                    "--treenode-filter", "/*/Cratis.Specifications.Probes/failing_fact_and_teardown/should_pass",
                    "--report-trx", "--report-trx-filename", "teardown.trx", "--results-directory", directory
                }
            }
        };
        process.Start();
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }

        var diagnostics = await output + await error;
        exit_code = process.ExitCode;
        var path = Path.Combine(directory, "teardown.trx");
        if (!File.Exists(path)) throw new InvalidOperationException(diagnostics);
        trx = XDocument.Load(path);
    }

    void Destroy() => Directory.Delete(directory, recursive: true);

    IEnumerable<XElement> Results => trx.Descendants().Where(_ => _.Name.LocalName == "UnitTestResult");

    void should_fail_the_mtp_run() => exit_code.ShouldEqual(2);
    void should_include_both_fact_and_teardown_despite_the_tree_filter() => Results.Count().ShouldEqual(2);
    void should_keep_the_fact_passed() => Results.Single(_ => (string)_.Attribute("outcome") == "Passed").Attribute("testName").Value.ShouldContain("should pass");
    void should_report_one_failed_teardown_node() => Results.Single(_ => (string)_.Attribute("outcome") == "Failed").Attribute("testName").Value.EndsWith(" (teardown)", StringComparison.Ordinal).ShouldBeTrue();
    void should_include_the_destroy_error() => Results.Single(_ => (string)_.Attribute("outcome") == "Failed").Value.ShouldContain("Destroy failed");
    void should_include_the_disposal_error() => Results.Single(_ => (string)_.Attribute("outcome") == "Failed").Value.ShouldContain("Dispose failed");
}
