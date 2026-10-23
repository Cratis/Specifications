// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldEqualityExtensions;

public class when_matching_with_a_callback : Specification
{
    Exception _matching;
    Exception _notMatching;

    void Because()
    {
        _matching = Catch.Exception(() => 5.ShouldMatch(_ => _ > 3));
        _notMatching = Catch.Exception(() => 2.ShouldMatch(_ => _ > 3));
    }

    [Test] public void should_pass_when_the_callback_accepts_the_value() => _matching.ShouldBeNull();
    [Test] public void should_fail_when_the_callback_rejects_the_value() => _notMatching.ShouldNotBeNull();
}
