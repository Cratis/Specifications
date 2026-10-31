// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_calls_an_iterator_helper : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        int _count;

        System.Collections.Generic.IEnumerable<int> Drain()
        {
            _count = 0;
            yield return 1;
        }

        [Xunit.Fact]
        void should_not_run_the_iterator()
        {
            Drain();
            var pending = Drain();
            _ = pending;
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
