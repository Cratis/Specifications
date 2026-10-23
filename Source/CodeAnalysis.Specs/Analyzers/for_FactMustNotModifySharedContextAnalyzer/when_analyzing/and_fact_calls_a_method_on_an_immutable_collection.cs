// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_calls_a_method_on_an_immutable_collection : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        System.Collections.Immutable.ImmutableList<int> _items;
        System.Collections.Immutable.ImmutableHashSet<int> _set;

        [Xunit.Fact]
        void should_not_change()
        {
            _items.Add(1);
            _items.Clear();
            _set.Remove(1);
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample"));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
