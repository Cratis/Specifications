// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using TUnit.Core.Interfaces;

namespace Cratis.Specifications;

/// <summary>
/// Represents a <see cref="IClassConstructor"/> that creates exactly one instance of each specification class,
/// so that all the facts of a specification share the same context.
/// </summary>
public class SpecificationClassConstructor : IClassConstructor
{
    static readonly ConcurrentDictionary<Type, Lazy<object>> _instances = new();

    /// <inheritdoc/>
    public Task<object> Create(Type type, ClassConstructorMetadata classConstructorMetadata) =>
        Task.FromResult(_instances.GetOrAdd(type, static t => new Lazy<object>(() => Activator.CreateInstance(t)!)).Value);

    /// <summary>
    /// Removes the shared instance for a specification class.
    /// </summary>
    /// <param name="type">The specification class.</param>
    /// <returns>The instance if one was created; otherwise null.</returns>
    internal static object? Release(Type type) =>
        _instances.TryRemove(type, out var instance) && instance.IsValueCreated ? instance.Value : null;
}
