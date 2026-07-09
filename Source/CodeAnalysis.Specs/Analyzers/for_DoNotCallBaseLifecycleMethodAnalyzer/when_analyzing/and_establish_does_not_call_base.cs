// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_DoNotCallBaseLifecycleMethodAnalyzer.when_analyzing;

public class and_establish_does_not_call_base : given.a_do_not_call_base_lifecycle_method_analyzer
{
    const string Usage = """
    public class a_context : Cratis.Specifications.Specification
    {
        protected void Establish() { }
    }

    public class when_doing_something : a_context
    {
        void Because() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<DoNotCallBaseLifecycleMethodAnalyzer>.VerifyAnalyzer(SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
