// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Immutable;

namespace Cratis.Specifications.Runner.Generator;

/// <summary>
/// Represents an immutable array compared by its elements, so incremental generator models cache correctly.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <param name="items">The elements.</param>
public readonly struct EquatableArray<T>(ImmutableArray<T> items) : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    readonly ImmutableArray<T> _items = items;

    /// <summary>
    /// Gets the number of elements.
    /// </summary>
    public int Count => _items.IsDefault ? 0 : _items.Length;

    /// <summary>
    /// Gets the element at an index.
    /// </summary>
    /// <param name="index">The index.</param>
    public T this[int index] => _items[index];

    /// <inheritdoc/>
    public bool Equals(EquatableArray<T> other) => this.SequenceEqual(other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var item in this)
        {
            hash = (hash * 31) + item.GetHashCode();
        }

        return hash;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => (_items.IsDefault ? [] : _items).AsEnumerable().GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
