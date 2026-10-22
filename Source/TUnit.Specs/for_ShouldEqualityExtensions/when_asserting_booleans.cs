// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldEqualityExtensions;

public class when_asserting_booleans : Specification
{
    Exception _trueIsTrue;
    Exception _falseIsTrue;
    Exception _falseIsFalse;
    Exception _trueIsFalse;

    void Because()
    {
        _trueIsTrue = Catch.Exception(() => true.ShouldBeTrue());
        _falseIsTrue = Catch.Exception(() => false.ShouldBeTrue());
        _falseIsFalse = Catch.Exception(() => false.ShouldBeFalse());
        _trueIsFalse = Catch.Exception(() => true.ShouldBeFalse());
    }

    [Test] public void should_pass_for_true_expected_true() => _trueIsTrue.ShouldBeNull();
    [Test] public void should_fail_for_false_expected_true() => _falseIsTrue.ShouldNotBeNull();
    [Test] public void should_pass_for_false_expected_false() => _falseIsFalse.ShouldBeNull();
    [Test] public void should_fail_for_true_expected_false() => _trueIsFalse.ShouldNotBeNull();
}
