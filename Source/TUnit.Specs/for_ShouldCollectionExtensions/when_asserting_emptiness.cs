// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldCollectionExtensions;

public class when_asserting_emptiness : Specification
{
    Exception _emptyIsEmpty;
    Exception _filledIsEmpty;
    Exception _filledIsNotEmpty;
    Exception _emptyIsNotEmpty;

    void Because()
    {
        _emptyIsEmpty = Catch.Exception(() => Array.Empty<int>().ShouldBeEmpty());
        _filledIsEmpty = Catch.Exception(() => new[] { 1 }.ShouldBeEmpty());
        _filledIsNotEmpty = Catch.Exception(() => new[] { 1 }.ShouldNotBeEmpty());
        _emptyIsNotEmpty = Catch.Exception(() => Array.Empty<int>().ShouldNotBeEmpty());
    }

    [Test] public void should_pass_for_an_empty_collection() => _emptyIsEmpty.ShouldBeNull();
    [Test] public void should_fail_for_a_filled_collection_expected_empty() => _filledIsEmpty.ShouldNotBeNull();
    [Test] public void should_pass_for_a_filled_collection_expected_filled() => _filledIsNotEmpty.ShouldBeNull();
    [Test] public void should_fail_for_an_empty_collection_expected_filled() => _emptyIsNotEmpty.ShouldNotBeNull();
}
