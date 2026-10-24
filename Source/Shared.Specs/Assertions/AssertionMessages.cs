// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Assertions;

/// <summary>
/// The exact failure messages every package must produce. Every package verifies all of them, so a message that differs
/// between packages shows up as a failing case.
/// </summary>
public static class AssertionMessages
{
    static readonly int[] _numbers = [1, 2, 3];
    static readonly int[] _reversed = [3, 2, 1];
    static readonly string _noString = null;

    /// <summary>
    /// Gets every failing assertion together with the message it must fail with.
    /// </summary>
    public static IReadOnlyList<(string Name, Action Run, string Message)> All { get; } =
    [
        ("equal", () => 1.ShouldEqual(2), $"Expected: 2{Environment.NewLine}Actual:   1"),
        ("equal params", () => _numbers.ShouldEqual(3, 2, 1), $"Expected: [3, 2, 1]{Environment.NewLine}Actual:   [1, 2, 3]"),
        ("in order", () => _numbers.ShouldEqualInOrder(_reversed), $"Expected: [3, 2, 1]{Environment.NewLine}Actual:   [1, 2, 3]"),
        ("contain only", () => _numbers.ShouldContainOnly(1, 2), "Collection '1,2,3' does not only contain '1,2'"),
        ("contain", () => _numbers.ShouldContain(4), $"Collection does not contain 4{Environment.NewLine}Collection: [1, 2, 3]"),
        ("greater than", () => 1.ShouldBeGreaterThan(2), "1 should be greater than 2"),
        ("true", () => false.ShouldBeTrue(), $"Expected: True{Environment.NewLine}Actual:   False"),
        ("null actual", () => _noString.ShouldContain("a"), "ShouldContain expected the actual value to contain the expected item but the actual value was null"),
        ("string not contain: actual is null", () => _noString.ShouldNotContain("a"), "ShouldNotContain expected the actual value not to contain the specified item but the actual value was null"),
        ("single matching", () => _numbers.ShouldContainSingleMatching(item => item > 1), "Expected exactly one item matching the filter but found 2"),
    ];

    /// <summary>
    /// Runs every case and returns a description of each case whose message differs from the expected one.
    /// </summary>
    /// <param name="catchFailure">Runs a case and returns the failure it threw; some frameworks need their own way of catching.</param>
    /// <returns>The mismatches, empty when every message is as expected.</returns>
    public static IReadOnlyList<string> Verify(Func<Action, Exception?> catchFailure)
    {
        var mismatches = new List<string>();
        foreach (var (name, run, message) in All)
        {
            var thrown = catchFailure(run);
            if (thrown?.Message != message)
            {
                mismatches.Add($"{name}: expected message '{message}' but got '{thrown?.Message ?? "no failure"}'");
            }
        }

        return mismatches;
    }
}
