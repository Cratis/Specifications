// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_base_class_is_marked_per_fact : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    [System.AttributeUsage(System.AttributeTargets.All)]
    public sealed class PerFactSpecificationLifecycleAttribute : System.Attribute
    {
    }

    [Sample.PerFactSpecificationLifecycle]
    public class base_specification : Cratis.Specifications.Specification
    {
        protected int _value;
    }

    public class when_something : base_specification
    {
        [Xunit.Fact]
        void should_change() => _value = 1;
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample"));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
