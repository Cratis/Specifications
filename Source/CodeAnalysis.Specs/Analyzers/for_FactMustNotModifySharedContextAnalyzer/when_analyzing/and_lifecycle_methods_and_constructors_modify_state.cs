// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_lifecycle_methods_and_constructors_modify_state : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        int _value;
        System.Collections.Generic.List<int> _items;

        public when_something() => _value = 3;

        void Establish() { _value = 1; _items = new(); _items.Add(1); }
        void Because() => _value++;
        void Destroy() => _items.Clear();

        [Xunit.Fact] void should_not_change() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.when_something"));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
