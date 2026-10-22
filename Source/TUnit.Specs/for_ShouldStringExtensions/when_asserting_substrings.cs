// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldStringExtensions;

public class when_asserting_substrings : Specification
{
    Exception _contained;
    Exception _missing;
    Exception _notContainedAbsent;
    Exception _notContainedPresent;

    void Because()
    {
        _contained = Catch.Exception(() => "hello world".ShouldContain("world"));
        _missing = Catch.Exception(() => "hello world".ShouldContain("moon"));
        _notContainedAbsent = Catch.Exception(() => "hello world".ShouldNotContain("moon"));
        _notContainedPresent = Catch.Exception(() => "hello world".ShouldNotContain("world"));
    }

    [Test] public void should_pass_when_the_substring_is_contained() => _contained.ShouldBeNull();
    [Test] public void should_fail_when_the_substring_is_missing() => _missing.Message.ShouldContain("moon");
    [Test] public void should_pass_when_the_substring_is_absent_and_not_expected() => _notContainedAbsent.ShouldBeNull();
    [Test] public void should_fail_when_the_substring_is_present_and_not_expected() => _notContainedPresent.ShouldNotBeNull();
}
