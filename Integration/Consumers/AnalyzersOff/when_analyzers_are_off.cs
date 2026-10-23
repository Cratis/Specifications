// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;

namespace Consumer.for_Analyzers;

// The fact below is deliberately not named should_*, which CRSPEC0001 reports as a warning. With
// CratisSpecificationsAnalyzers=false the analyzer must not run, so this project builds under TreatWarningsAsErrors.
public class when_analyzers_are_off : Specification
{
    bool _ran;
    void When() => _ran = true;

    [Xunit.Fact]
    public void runs_without_the_analyzer_complaining() => _ran.ShouldBeTrue();
}
