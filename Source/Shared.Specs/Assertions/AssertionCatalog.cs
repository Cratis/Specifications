// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Assertions;

/// <summary>
/// Runs the shared assertion cases and reports every case that did not behave as catalogued.
/// </summary>
public static class AssertionCatalog
{
    /// <summary>
    /// Runs every case.
    /// </summary>
    /// <param name="failureType">The assertion exception type the package under test fails with. Derived types qualify.</param>
    /// <param name="catchFailure">Runs an assertion that is expected to fail and returns what it threw, or null when it did not throw.
    /// The default is a plain try/catch; NUnit supplies its own because it records caught failures on the current test.</param>
    /// <returns>One line per case that did not behave as catalogued; empty when all did.</returns>
    public static IReadOnlyList<string> Verify(Type failureType, Func<Action, Exception?>? catchFailure = null)
    {
        catchFailure ??= Catch;
        var mismatches = new List<string>();

        foreach (var assertion in AssertionCases.All)
        {
            var thrown = assertion.Expected == ExpectedOutcome.Pass ? Catch(assertion.Run) : catchFailure(assertion.Run);

            if (assertion.Expected == ExpectedOutcome.Pass && thrown is not null)
            {
                mismatches.Add($"{assertion.Name}: expected to pass but threw {thrown.GetType().Name}: {thrown.Message}");
            }
            else if (assertion.Expected == ExpectedOutcome.Fail && thrown is null)
            {
                mismatches.Add($"{assertion.Name}: expected to fail but passed");
            }
            else if (assertion.Expected == ExpectedOutcome.Fail && !failureType.IsInstanceOfType(thrown))
            {
                mismatches.Add($"{assertion.Name}: expected to fail with {failureType.Name} but threw {thrown!.GetType().Name}: {thrown.Message}");
            }
        }

        return mismatches;
    }

    static Exception? Catch(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
