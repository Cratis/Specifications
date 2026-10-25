// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_verifies_in_order_with_an_async_lambda : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        System.Collections.Generic.ICollection<string> _store;

        [Xunit.Fact]
        async System.Threading.Tasks.Task should_remove() => await NSubstitute.Received.InOrder(async () =>
        {
            await System.Threading.Tasks.Task.CompletedTask;
            _store.Remove("x");
        });
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage) + InOrderSource);

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
