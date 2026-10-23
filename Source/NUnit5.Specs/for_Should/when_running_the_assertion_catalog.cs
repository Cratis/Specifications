// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Assertions;

namespace Cratis.Specifications.for_Should;

public class when_running_the_assertion_catalog : Specification
{
    IReadOnlyList<string> _mismatches;

    void Because() => _mismatches = AssertionCatalog.Verify(typeof(AssertionException), action => Assert.Catch(() => action()));

    [Test] public void should_behave_as_catalogued_for_every_case() => string.Join(Environment.NewLine, _mismatches).ShouldEqual(string.Empty);
    [Test] public void should_have_catalogued_cases() => AssertionCases.All.ShouldNotBeEmpty();
}
