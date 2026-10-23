// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_builds_collections_in_locals : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        [Xunit.Fact]
        void should_not_change()
        {
            var builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<int>();
            builder.Add(1);
            System.Collections.Generic.List<int> list = [1, 2];
            var initialized = new System.Collections.Generic.List<int> { 1, 2 };
            list.Add(3);
            initialized.Clear();
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample"));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
