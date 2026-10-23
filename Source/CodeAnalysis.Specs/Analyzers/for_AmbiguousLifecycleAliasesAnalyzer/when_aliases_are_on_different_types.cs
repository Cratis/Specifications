// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_AmbiguousLifecycleAliasesAnalyzer;

public class when_aliases_are_on_different_types : Specification
{
    Task _result;
    void Because() => _result = AnalyzerVerifier<AmbiguousLifecycleAliasesAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap("""
        public class a_context : Cratis.Specifications.Specification { void Given() { } } public class when_acting : a_context { void Establish() { } }
        """));
    [Fact] Task should_apply_the_lifecycle_rule() => _result;
}
