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
    static readonly int[] _large = [.. Enumerable.Range(0, 40).Select(index => index % 10)];
    static readonly int[] _largeReversed = [.. _large.Reverse()];
    static readonly int[] _largeOneDifferent = [.. _large.Take(39), 10];
    static readonly int[] _largeLonger = [.. _large, 0];
    static readonly string?[] _largeWithNulls = [.. Enumerable.Repeat("a", 30), null, null];
    static readonly string?[] _largeWithNullsShuffled = [null, .. Enumerable.Repeat("a", 30), null];
    static readonly string?[] _largeWithOneNull = [.. Enumerable.Repeat("a", 31), null];
    static readonly UnhashableItem[] _largeUnhashable = [.. Enumerable.Range(0, 20).Select(index => new UnhashableItem(index))];
    static readonly UnhashableItem[] _largeUnhashableReversed = [.. _largeUnhashable.Reverse()];
    static readonly Dictionary<string, int> _dictionary = new() { ["one"] = 1 };
    static readonly int[][] _nestedCopy = [[1], [2]];
    static readonly int[][] _nestedReversedCopy = [[2], [1]];
    static readonly List<List<int>> _nestedLists = [[1, 2], [3]];
    static readonly List<List<int>> _nestedListsCopy = [[1, 2], [3]];
    static readonly int[] _firstInnerCopy = [1];
    static readonly int[] _secondInnerCopy = [2];
    static readonly HashSet<int> _set = [1, 2, 3];
    static readonly HashSet<int> _setReversed = [3, 2, 1];
    static readonly HashSet<int> _setShorter = [1, 2];
    static readonly HashSet<int> _setDifferent = [1, 3];
    static readonly HashSet<int> _setSingle = [1];
    static readonly HashSet<int> _setPairReversed = [2, 1];
    static readonly EquatableCollection[] _nestedEquatable = [new(1, 1, 2)];
    static readonly EquatableCollection[] _nestedEquatableSame = [new(1, 2, 1)];
    static readonly EquatableCollection[] _nestedEquatableDifferent = [new(2, 1, 2)];
    static readonly Dictionary<string, int>[] _nestedDictionary = [new() { ["one"] = 1, ["two"] = 2 }];
    static readonly HashSet<int>[] _nestedSet = [_setShorter];

    // Delegates typed as Func, which every predicate-taking assertion accepts.
    static readonly Func<int, bool> _greaterThanOne = item => item > 1;
    static readonly Func<int, bool> _greaterThanTwo = item => item > 2;
    static readonly Func<int, bool> _positive = item => item > 0;

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
        // Params are ordered in every package.
        Pass("equal params: same sequence", () => _numbers.ShouldEqual(1, 2, 3)),
        Fail("equal params: reversed sequence", () => _numbers.ShouldEqual(3, 2, 1)),
        Fail("equal params: different items", () => _numbers.ShouldEqual(1, 2, 4)),
        Pass("contain only: reversed sequence", () => _numbers.ShouldContainOnly(3, 2, 1)),
        Fail("contain only: duplicates that differ", () => _duplicated.ShouldContainOnly(1, 2)),
        Fail("contain only: extra item", () => _numbers.ShouldContainOnly(1, 2)),
        Fail("contain only: actual is null", () => _noCollection.ShouldContainOnly(1)),
        Pass("contain only: many items with duplicates in another order", () => _large.ShouldContainOnly(_largeReversed)),
        Fail("contain only: many items with one different item", () => _large.ShouldContainOnly(_largeOneDifferent)),
        Fail("contain only: many items with one extra item", () => _large.ShouldContainOnly(_largeLonger)),
        Pass("contain only: many items with nulls in another place", () => _largeWithNulls.ShouldContainOnly(_largeWithNullsShuffled)),
        Fail("contain only: many items with a different number of nulls", () => _largeWithNulls.ShouldContainOnly(_largeWithOneNull)),
        Pass("contain only: many items whose hash code throws", () => _largeUnhashable.ShouldContainOnly(_largeUnhashableReversed)),

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
        Fail("in range: actual is null", () => ((string)null).ShouldBeInRange("a", "z")),
        Fail("not in range: actual is null", () => ((string)null).ShouldNotBeInRange("a", "z")),

        // Nested collections compare deeply, not by reference.
        Pass("in order: nested collections holding equal copies", () => _nested.ShouldEqualInOrder(_nestedCopy)),
        Pass("equal: nested collections holding equal copies", () => _nested.ShouldEqual(_nestedCopy)),
        Pass("equal params: nested collections holding equal copies", () => _nested.ShouldEqual(_firstInnerCopy, _secondInnerCopy)),
        Fail("equal params: nested collections in a different order", () => _nested.ShouldEqual(_secondInnerCopy, _firstInnerCopy)),
        Pass("equal: nested lists holding equal copies", () => _nestedLists.ShouldEqual(_nestedListsCopy)),
        Fail("in order: nested collections in a different order", () => _nested.ShouldEqualInOrder(_nestedReversedCopy)),
        Pass("any order: nested collections holding equal copies", () => _nested.ShouldEqualInAnyOrder(_nestedReversedCopy)),
        Pass("contain only: nested collections holding equal copies", () => _nested.ShouldContainOnly(_nestedReversedCopy)),
        Pass("contain: nested collection holding an equal copy", () => _nested.ShouldContain(_secondInnerCopy)),
        Fail("not contain: nested collection holding an equal copy", () => _nested.ShouldNotContain(_secondInnerCopy)),

        // Equality precedence and unordered collection kinds, including object-typed nested values.
        Pass("equal: dictionaries in different insertion order", () => new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 }.ShouldEqual(new Dictionary<string, int> { ["two"] = 2, ["one"] = 1 })),
        Fail("equal: dictionaries with a different value", () => new Dictionary<string, int> { ["one"] = 1 }.ShouldEqual(new Dictionary<string, int> { ["one"] = 2 })),
        Fail("equal: dictionaries with a different key", () => new Dictionary<string, int> { ["one"] = 1 }.ShouldEqual(new Dictionary<string, int> { ["two"] = 1 })),
        Fail("equal: dictionaries with an extra entry", () => new Dictionary<string, int> { ["one"] = 1 }.ShouldEqual(new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 })),
        Pass("equal: non-generic dictionaries in different order", () => ((object)new System.Collections.Hashtable { ["one"] = 1, ["two"] = 2 }).ShouldEqual(new System.Collections.Specialized.OrderedDictionary { ["two"] = 2, ["one"] = 1 })),
        Pass("equal: readonly dictionaries compare deeply in any order", () => new ReadOnlyMap([new("one", [1, 2]), new("two", [3])]).ShouldEqual(new ReadOnlyMap([new("two", [3]), new("one", [1, 2])]))),
        Fail("equal: readonly dictionaries with different nested values", () => new ReadOnlyMap([new("one", [1, 2])]).ShouldEqual(new ReadOnlyMap([new("one", [2, 1])]))),
        Pass("equal: hash sets in different insertion order", () => _set.ShouldEqual(_setReversed)),
        Fail("equal: hash sets with different items", () => _setShorter.ShouldEqual(_setDifferent)),
        Fail("equal: hash sets with an extra item", () => _setSingle.ShouldEqual(_setShorter)),
        Pass("equal: equatable enumerable ignores element order", () => new EquatableCollection(1, 1, 2).ShouldEqual(new EquatableCollection(1, 2, 1))),
        Fail("equal: equatable enumerable rejects identical elements", () => new EquatableCollection(1, 1, 2).ShouldEqual(new EquatableCollection(2, 1, 2))),
        Pass("equal: object-typed equatable enumerable", () => ((object)new EquatableCollection(1, 1, 2)).ShouldEqual(new EquatableCollection(1, 2, 1))),
        Fail("equal: object-typed equatable enumerable differs", () => ((object)new EquatableCollection(1, 1, 2)).ShouldEqual(new EquatableCollection(2, 1, 2))),
        Pass("equal: nested equatable enumerable", () => _nestedEquatable.ShouldEqual(_nestedEquatableSame)),
        Fail("equal: nested equatable enumerable differs", () => _nestedEquatable.ShouldEqual(_nestedEquatableDifferent)),
        Pass("equal: list override does not reject equal elements", () => new EqualsList(1, 1, 2).ShouldEqual(new EqualsList(2, 1, 2))),
        Fail("equal: list override does not accept reversed elements", () => new EqualsList(1, 1, 2).ShouldEqual(new EqualsList(1, 2, 1))),
        Pass("contain: nested dictionaries ignore insertion order", () => _nestedDictionary.ShouldContain(new Dictionary<string, int> { ["two"] = 2, ["one"] = 1 })),
        Pass("contain: nested sets ignore insertion order", () => _nestedSet.ShouldContain(_setPairReversed)),
        Fail("not equal: equal dictionaries ignore insertion order", () => new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 }.ShouldNotEqual(new Dictionary<string, int> { ["two"] = 2, ["one"] = 1 })),

        // Null rules.
        Pass("equal: null expected and null actual", () => _noString.ShouldEqual(null)),
        Fail("equal: null actual", () => _noString.ShouldEqual("a")),
        Fail("equal: null expected", () => "a".ShouldEqual(null)),
        Pass("equal enumerable: null expected and null actual", () => _noCollection.ShouldEqual(_noCollection)),
        Fail("equal enumerable: actual is null", () => _noCollection.ShouldEqual((IEnumerable<int>)_numbers)),
        Fail("equal enumerable: expected is null", () => _numbers.ShouldEqual(_noCollection)),
        Fail("contain only: expected is null", () => _numbers.ShouldContainOnly(_noCollection)),
        Fail("contain: expected collection is null", () => _numbers.ShouldContain(_noCollection)),
        Pass("null: actual is null", _noString.ShouldBeNull),
        Fail("null: actual is not null", "a".ShouldBeNull),
        Fail("not null: actual is null", _noString.ShouldNotBeNull),
        Fail("assignable: actual is null", _noString.ShouldBeAssignableFrom<string>),
        Fail("exact type: actual is null", _noString.ShouldBeOfExactType<string>),

        // Func delegates.
        Pass("contain: Func matches", () => _numbers.ShouldContain(_greaterThanTwo)),
        Fail("not contain: Func matches", () => _numbers.ShouldNotContain(_greaterThanTwo)),
        Pass("single matching: Func matches once", () => _numbers.ShouldContainSingleMatching(_greaterThanTwo)),
        Fail("single matching: Func matches twice", () => _numbers.ShouldContainSingleMatching(_greaterThanOne)),
        Pass("each conforms: Func holds for all", () => _numbers.ShouldEachConformTo(_positive)),
        Fail("each conforms: Func fails for one", () => _numbers.ShouldEachConformTo(_greaterThanOne)),
        Pass("match: Func holds", () => 2.ShouldMatch(_greaterThanOne)),
        Fail("match: Func does not hold", () => 1.ShouldMatch(_greaterThanOne)),
    ];

    static AssertionCase Pass(string name, Action run) => new(name, run, ExpectedOutcome.Pass);

    static AssertionCase Fail(string name, Action run) => new(name, run, ExpectedOutcome.Fail);

    sealed class EquatableCollection(int identity, params int[] items) : IEnumerable<int>, IEquatable<EquatableCollection>
    {
        readonly int _identity = identity;

        public bool Equals(EquatableCollection? other) => other is not null && _identity == other._identity;

        public override bool Equals(object? obj) => obj is EquatableCollection other && Equals(other);

        public override int GetHashCode() => _identity;

        public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)items).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    sealed class EqualsList(int identity, params int[] items) : List<int>(items)
    {
        readonly int _identity = identity;

        public override bool Equals(object? obj) => obj is EqualsList other && _identity == other._identity;

        public override int GetHashCode() => _identity;
    }

    sealed class ReadOnlyMap(KeyValuePair<string, int[]>[] entries) : IReadOnlyDictionary<string, int[]>
    {
        public int Count => entries.Length;

        public IEnumerable<string> Keys => entries.Select(entry => entry.Key);

        public IEnumerable<int[]> Values => entries.Select(entry => entry.Value);

        public int[] this[string key] => entries.First(entry => entry.Key == key).Value;

        public bool ContainsKey(string key) => entries.Any(entry => entry.Key == key);

        public bool TryGetValue(string key, out int[] value)
        {
            value = entries.FirstOrDefault(entry => entry.Key == key).Value;
            return value is not null;
        }

        public IEnumerator<KeyValuePair<string, int[]>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, int[]>>)entries).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    sealed class UnhashableItem(int value)
    {
        public int Value { get; } = value;

        public override bool Equals(object? obj) => obj is UnhashableItem other && other.Value == Value;

        public override int GetHashCode() => throw new NotSupportedException("Not hashable");
    }
}
