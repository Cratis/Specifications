// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_AmbiguousLifecycleAliasesAnalyzer;

public class when_an_alias_is_static_or_parameterized : Specification
{
    Task _result;
    void Because() => _result = AnalyzerVerifier<AmbiguousLifecycleAliasesAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap("""
        public class when_acting : Cratis.Specifications.Specification { void Establish() { } static void Given() { } void When(int value) { } void Because() { } }
        """));
    [Fact] Task should_apply_the_lifecycle_rule() => _result;
}
