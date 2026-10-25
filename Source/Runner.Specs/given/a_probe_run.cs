// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.given;

/// <summary>
/// Runs a probe specification through the class runner.
/// </summary>
/// <typeparam name="TProbe">The probe specification.</typeparam>
public class a_probe_run<TProbe> : Specification
{
    private protected SpecificationWork work;
    private protected FactResult[] results;
    private protected IReadOnlyList<string> calls;
    private protected FactResult? teardown_failure;

    void Establish()
    {
        CallLog.Clear(typeof(TProbe).Name);
        work = the_probes.WorkFor<TProbe>();
    }

    async Task Because()
    {
        var result = await SpecificationClassRunner.Run(work, sequentialFacts: false);
        results = result.Facts;
        teardown_failure = result.TearDownFailure;
        calls = CallLog.For(typeof(TProbe).Name);
    }

    private protected FactResult Result(string method) => the_probes.ResultOf(work, results, method);
}
