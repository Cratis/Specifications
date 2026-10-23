// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Describes a fact of a specification class, as emitted by the source generator. Not intended to be used directly.
/// </summary>
/// <param name="methodName">The name of the fact method.</param>
/// <param name="displayName">The readable Given/When/Then name of the fact.</param>
/// <param name="filePath">The source file declaring the fact.</param>
/// <param name="line">The 1-based line of the fact declaration.</param>
/// <param name="invoke">Invokes the fact on an instance of the specification class.</param>
/// <param name="skipReason">The reason the fact is skipped; null when it runs.</param>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class FactDescriptor(string methodName, string displayName, string? filePath, int line, Func<object, ValueTask> invoke, string? skipReason = null)
{
    /// <summary>Gets the method name.</summary>
    public string MethodName { get; } = methodName;

    /// <summary>Gets the readable display name.</summary>
    public string DisplayName { get; } = displayName;

    /// <summary>Gets the source file path.</summary>
    public string? FilePath { get; } = filePath;

    /// <summary>Gets the 1-based line number.</summary>
    public int Line { get; } = line;

    /// <summary>Gets the invoker.</summary>
    public Func<object, ValueTask> Invoke { get; } = invoke;

    /// <summary>Gets the skip reason.</summary>
    public string? SkipReason { get; } = skipReason;

    /// <summary>Gets the stable unique identifier; assigned on registration.</summary>
    internal string Uid { get; set; } = methodName;

    /// <summary>Gets the immutable node properties shared by every message about this fact; assigned on registration.</summary>
    internal IProperty[] Properties { get; set; } = [];
}
