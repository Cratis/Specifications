// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Describes a specification class, as emitted by the source generator. Not intended to be used directly.
/// </summary>
/// <param name="uid">The stable unique identifier: the fully qualified type name.</param>
/// <param name="namespace">The namespace of the specification class.</param>
/// <param name="typeName">The type name, with containing types separated by '+'.</param>
/// <param name="displayName">The readable Given/When/Then name derived from the namespace and type hierarchy.</param>
/// <param name="create">Creates an instance; null when the class has a discovery error.</param>
/// <param name="establish">Runs the Establish (or Given) chain, most basic type first; null when there is none.</param>
/// <param name="because">Runs the Because (or When) chain, most basic type first; null when there is none.</param>
/// <param name="destroy">Runs the Destroy chain, most basic type first; null when there is none.</param>
/// <param name="facts">The facts of the class.</param>
/// <param name="error">A discovery error that fails every fact of the class without running anything.</param>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class SpecificationClassDescriptor(
    string uid,
    string @namespace,
    string typeName,
    string displayName,
    Func<object>? create,
    Func<object, ValueTask>? establish,
    Func<object, ValueTask>? because,
    Func<object, ValueTask>? destroy,
    IReadOnlyList<FactDescriptor> facts,
    string? error = null)
{
    /// <summary>Gets the stable unique identifier.</summary>
    public string Uid { get; } = uid;

    /// <summary>Gets the namespace.</summary>
    public string Namespace { get; } = @namespace;

    /// <summary>Gets the type name, with containing types separated by '+'.</summary>
    public string TypeName { get; } = typeName;

    /// <summary>Gets the readable display name.</summary>
    public string DisplayName { get; } = displayName;

    /// <summary>Gets the factory creating the instance.</summary>
    public Func<object>? Create { get; } = create;

    /// <summary>Gets the Establish chain.</summary>
    public Func<object, ValueTask>? Establish { get; } = establish;

    /// <summary>Gets the Because chain.</summary>
    public Func<object, ValueTask>? Because { get; } = because;

    /// <summary>Gets the Destroy chain.</summary>
    public Func<object, ValueTask>? Destroy { get; } = destroy;

    /// <summary>Gets the facts.</summary>
    public IReadOnlyList<FactDescriptor> Facts { get; } = facts;

    /// <summary>Gets the discovery error, if any.</summary>
    public string? Error { get; } = error;
}
