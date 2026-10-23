// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_only_uses_locals_and_reads : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        int _value;
        System.Collections.Generic.List<int> _items;

        [Xunit.Fact]
        void should_not_change()
        {
            var local = 0;
            local = _value;
            local++;
            var items = new System.Collections.Generic.List<int>();
            items.Add(_items.Count);
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.when_something"));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
