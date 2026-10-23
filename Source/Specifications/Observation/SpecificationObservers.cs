// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Holds the registered <see cref="ISpecificationObserver"/> instances.
/// </summary>
/// <remarks>
/// The adapters check <see cref="Any"/> once per specification instance; when it is false nothing else of the seam
/// runs, so an unobserved run pays for one static field read per instance.
/// </remarks>
internal static class SpecificationObservers
{
    static readonly Lock _lock = new();
    static ISpecificationObserver[] _observers = [];
    static volatile bool _any;

    /// <summary>
    /// Gets a value indicating whether any observer is registered.
    /// </summary>
    public static bool Any => _any;

    /// <summary>
    /// Gets a snapshot of the registered observers.
    /// </summary>
    internal static ISpecificationObserver[] All => Volatile.Read(ref _observers);

    /// <summary>
    /// Registers an observer.
    /// </summary>
    /// <param name="observer">The observer to register.</param>
    public static void Add(ISpecificationObserver observer)
    {
        lock (_lock)
        {
            Volatile.Write(ref _observers, [.. _observers, observer]);
            _any = true;
        }
    }

    /// <summary>
    /// Removes a registered observer.
    /// </summary>
    /// <param name="observer">The observer to remove.</param>
    public static void Remove(ISpecificationObserver observer)
    {
        lock (_lock)
        {
            Volatile.Write(ref _observers, Array.FindAll(_observers, registered => !ReferenceEquals(registered, observer)));
            _any = _observers.Length > 0;
        }
    }
}
