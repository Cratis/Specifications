// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.ExceptionServices;

namespace Cratis.Specifications;

/// <summary>
/// Fans out the end of the test session to the callbacks registered for it.
/// </summary>
/// <remarks>
/// Every adapter calls <see cref="End"/> once after the last specification has run: the xUnit assembly runners,
/// the NUnit and TUnit session hooks the packages compile into the test project, and the native runner.
/// With nothing registered, <see cref="End"/> returns a completed task without allocating.
/// </remarks>
internal static class SpecificationSession
{
    static readonly Lock _lock = new();
    static List<Func<ValueTask>>? _callbacks;

    /// <summary>
    /// Registers a callback to run when the test session ends.
    /// </summary>
    /// <param name="onEnd">The callback.</param>
    public static void Register(Func<ValueTask> onEnd)
    {
        lock (_lock)
        {
            (_callbacks ??= []).Add(onEnd);
        }
    }

    /// <summary>
    /// Runs and removes the registered callbacks, the last registered first.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Every callback runs even when an earlier one fails; the failures are thrown afterwards, as is when only one
    /// callback failed and as an <see cref="AggregateException"/> otherwise.
    /// </remarks>
    public static ValueTask End()
    {
        if (Volatile.Read(ref _callbacks) is null) return default;

        Func<ValueTask>[] callbacks;
        lock (_lock)
        {
            if (_callbacks is null) return default;
            callbacks = [.. _callbacks];
            _callbacks = null;
        }

        return RunCallbacks(callbacks);
    }

    static async ValueTask RunCallbacks(Func<ValueTask>[] callbacks)
    {
        List<Exception>? failures = null;
        for (var index = callbacks.Length - 1; index >= 0; index--)
        {
            try
            {
                await callbacks[index]();
            }
            catch (Exception ex)
            {
                (failures ??= []).Add(ex);
            }
        }

        if (failures is null) return;
        if (failures.Count == 1) ExceptionDispatchInfo.Throw(failures[0]);
        throw new AggregateException(failures);
    }
}
