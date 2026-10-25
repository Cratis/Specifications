// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_verifies_received_call_chains : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public static class SubstituteExtensions
    {
        public static T Received<T>(this T substitute) => substitute;
        public static T DidNotReceive<T>(this T substitute) => substitute;
    }

    public class when_something : Cratis.Specifications.Specification
    {
        System.Collections.Generic.ICollection<string> _store;

        [Xunit.Fact]
        void should_remove()
        {
            _store.Received().Remove("x");
            _store.DidNotReceive().Clear();
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
