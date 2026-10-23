// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Assertions;

/// <summary>
/// The framework-free catalog of assertion examples. Every package runs all of them, so a difference between
/// packages shows up as a failing case instead of a surprise in somebody's specs.
/// </summary>
public static class AssertionCases
{
    static readonly int[] _numbers = [1, 2, 3];
    static readonly int[] _reversed = [3, 2, 1];
    static readonly int[] _duplicated = [1, 1, 2];
    static readonly int[] _duplicatedOther = [1, 2, 2];
    static readonly int[] _duplicatedShuffled = [2, 1, 1];
    static readonly int[] _shorter = [1, 2];
    static readonly int[] _longer = [1, 2, 3, 4];
    static readonly int[] _single = [1];
    static readonly int[] _empty = [];
    static readonly string?[] _withNull = ["a", null];
    static readonly string?[] _nullFirst = [null, "a"];
    static readonly int[] _firstInner = [1];
    static readonly int[] _secondInner = [2];
    static readonly int[] _otherInner = [3];
    static readonly int[][] _nested = [_firstInner, _secondInner];
    static readonly int[][] _nestedSame = [_firstInner, _secondInner];
    static readonly int[][] _nestedDifferent = [_firstInner, _otherInner];
    static readonly Dictionary<string, int> _dictionary = new() { ["one"] = 1 };

    // Null actuals: every assertion must fail with the package's own assertion exception.
    static readonly IEnumerable<int> _noCollection = null;
    static readonly IDictionary<string, int> _noDictionary = null;
    static readonly string _noString = null;
    static readonly IComparable _noComparable = null;

    /// <summary>
    /// Gets every catalogued assertion.
    /// </summary>
    public static IReadOnlyList<AssertionCase> All { get; } =
    [
        // Explicit ordering.
        Pass("in order: same sequence", () => _numbers.ShouldEqualInOrder(1, 2, 3)),
        Pass("in order: same sequence as enumerable", () => _numbers.ShouldEqualInOrder((IEnumerable<int>)[1, 2, 3])),
        Fail("in order: reversed sequence", () => _numbers.ShouldEqualInOrder(_reversed)),
        Fail("in order: shorter expected", () => _numbers.ShouldEqualInOrder(_shorter)),
        Fail("in order: longer expected", () => _numbers.ShouldEqualInOrder(_longer)),
        Pass("in order: element null in both", () => _withNull.ShouldEqualInOrder("a", null)),
        Fail("in order: element null in a different place", () => _withNull.ShouldEqualInOrder(_nullFirst)),
        Pass("in order: nested collections holding the same instances", () => _nested.ShouldEqualInOrder(_nestedSame)),
        Fail("in order: nested collections holding different items", () => _nested.ShouldEqualInOrder(_nestedDifferent)),
        Fail("in order: expected is null", () => _numbers.ShouldEqualInOrder(_noCollection)),
        Fail("in order: actual is null", () => _noCollection.ShouldEqualInOrder(_numbers)),

        // Explicit any order.
        Pass("any order: reversed sequence", () => _numbers.ShouldEqualInAnyOrder(_reversed)),
        Pass("any order: params", () => _numbers.ShouldEqualInAnyOrder(2, 3, 1)),
        Pass("any order: duplicates that match", () => _duplicated.ShouldEqualInAnyOrder(_duplicatedShuffled)),
        Fail("any order: duplicates that differ", () => _duplicated.ShouldEqualInAnyOrder(_duplicatedOther)),
        Fail("any order: missing item", () => _numbers.ShouldEqualInAnyOrder(_shorter)),
        Fail("any order: extra item", () => _numbers.ShouldEqualInAnyOrder(_longer)),
        Pass("any order: element null", () => _withNull.ShouldEqualInAnyOrder(_nullFirst)),
        Fail("any order: actual is null", () => _noCollection.ShouldEqualInAnyOrder(_numbers)),

        // Existing equality and contain-only.
        Pass("equal enumerable: same sequence", () => _numbers.ShouldEqual((IEnumerable<int>)[1, 2, 3])),
        Fail("equal enumerable: reversed sequence", () => _numbers.ShouldEqual((IEnumerable<int>)_reversed)),
#if NUNIT4
        // The NUnit 4 package has always compared params without regard to order.
        Pass("equal params: reversed sequence", () => _numbers.ShouldEqual(3, 2, 1)),
#else
        Fail("equal params: reversed sequence", () => _numbers.ShouldEqual(3, 2, 1)),
#endif
        Fail("equal params: different items", () => _numbers.ShouldEqual(1, 2, 4)),
        Pass("contain only: reversed sequence", () => _numbers.ShouldContainOnly(3, 2, 1)),
        Fail("contain only: duplicates that differ", () => _duplicated.ShouldContainOnly(1, 2)),
        Fail("contain only: extra item", () => _numbers.ShouldContainOnly(1, 2)),
        Fail("contain only: actual is null", () => _noCollection.ShouldContainOnly(1)),

        // Containment.
        Pass("contain: item present", () => _numbers.ShouldContain(2)),
        Fail("contain: item absent", () => _numbers.ShouldContain(4)),
        Pass("contain: all items present", () => _numbers.ShouldContain(1, 3)),
        Fail("contain: one item absent", () => _numbers.ShouldContain(1, 4)),
        Pass("contain: repeated expected item is not demanded repeatedly", () => _single.ShouldContain(1, 1)),
        Pass("contain: predicate matches", () => _numbers.ShouldContain(item => item > 2)),
        Fail("contain: predicate matches nothing", () => _numbers.ShouldContain(item => item > 3)),
        Pass("not contain: item absent", () => _numbers.ShouldNotContain(4)),
        Fail("not contain: item present", () => _numbers.ShouldNotContain(2)),
        Pass("not contain: predicate matches nothing", () => _numbers.ShouldNotContain(item => item > 3)),
        Fail("not contain: predicate matches", () => _numbers.ShouldNotContain(item => item > 2)),
        Pass("contain: dictionary key present", () => _dictionary.ShouldContain("one")),
        Fail("contain: dictionary key absent", () => _dictionary.ShouldContain("two")),
        Pass("not contain: dictionary key absent", () => _dictionary.ShouldNotContain("two")),
        Fail("not contain: dictionary key present", () => _dictionary.ShouldNotContain("one")),
        Fail("contain: actual is null", () => _noCollection.ShouldContain(1)),
        Fail("contain: predicate on a null actual", () => _noCollection.ShouldContain(item => item > 0)),
        Fail("not contain: actual is null", () => _noCollection.ShouldNotContain(1)),
        Fail("not contain: predicate on a null actual", () => _noCollection.ShouldNotContain(item => item > 0)),
        Fail("contain: dictionary is null", () => _noDictionary.ShouldContain("one")),
        Fail("not contain: dictionary is null", () => _noDictionary.ShouldNotContain("one")),

        // Single, empty and conforming.
        Pass("single item: one item", _single.ShouldContainSingleItem),
        Fail("single item: two items", _shorter.ShouldContainSingleItem),
        Fail("single item: actual is null", _noCollection.ShouldContainSingleItem),
        Pass("single matching: exactly one match", () => _numbers.ShouldContainSingleMatching(item => item == 2)),
        Fail("single matching: no match", () => _numbers.ShouldContainSingleMatching(item => item == 4)),
        Fail("single matching: two matches", () => _numbers.ShouldContainSingleMatching(item => item > 1)),
        Fail("single matching: actual is null", () => _noCollection.ShouldContainSingleMatching(item => item > 1)),
        Pass("empty: no items", _empty.ShouldBeEmpty),
        Fail("empty: has items", _numbers.ShouldBeEmpty),
        Fail("empty: actual is null", _noCollection.ShouldBeEmpty),
        Pass("not empty: has items", _numbers.ShouldNotBeEmpty),
        Fail("not empty: no items", _empty.ShouldNotBeEmpty),
        Fail("not empty: actual is null", _noCollection.ShouldNotBeEmpty),
        Pass("each conforms: all conform", () => _numbers.ShouldEachConformTo(item => item > 0)),
        Fail("each conforms: one does not", () => _numbers.ShouldEachConformTo(item => item > 1)),
        Fail("each conforms: actual is null", () => _noCollection.ShouldEachConformTo(item => item > 0)),

        // Strings.
        Pass("string contain: substring present", () => "hello world".ShouldContain("lo wo")),
        Fail("string contain: substring absent", () => "hello world".ShouldContain("xyz")),
        Fail("string contain: actual is null", () => _noString.ShouldContain("a")),
        Pass("string not contain: substring absent", () => "hello world".ShouldNotContain("xyz")),
        Fail("string not contain: substring present", () => "hello world".ShouldNotContain("lo wo")),
        Fail("string not contain: actual is null", () => _noString.ShouldNotContain("a")),

        // Comparables.
        Pass("greater than: greater", () => 2.ShouldBeGreaterThan(1)),
        Fail("greater than: smaller", () => 1.ShouldBeGreaterThan(2)),
        Fail("greater than: actual is null", () => _noComparable.ShouldBeGreaterThan(1)),
        Pass("less than: smaller", () => 1.ShouldBeLessThan(2)),
        Fail("less than: greater", () => 2.ShouldBeLessThan(1)),
        Fail("less than: actual is null", () => _noComparable.ShouldBeLessThan(1)),
        Pass("in range: inside", () => 2.ShouldBeInRange(1, 3)),
        Fail("in range: outside", () => 4.ShouldBeInRange(1, 3)),
    ];

    static AssertionCase Pass(string name, Action run) => new(name, run, ExpectedOutcome.Pass);

    static AssertionCase Fail(string name, Action run) => new(name, run, ExpectedOutcome.Fail);
}
