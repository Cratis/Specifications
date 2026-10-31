// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer;

public class when_creating_analyzer : Specification
{
    FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer _analyzer;

    void Establish() => _analyzer = new FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer();

    [Fact] void should_support_crspec0012() => _analyzer.SupportedDiagnostics.Any(d => d.Id == DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext).ShouldBeTrue();
    [Fact] void should_be_a_warning_by_default() => _analyzer.SupportedDiagnostics[0].DefaultSeverity.ShouldEqual(Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
}
