// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Configures how specifications share a resource. Put it on the class implementing <see cref="ISpecificationResource"/>.
/// </summary>
/// <remarks>
/// Without the attribute a resource has its own unlimited group and is stopped within 30 seconds.
/// </remarks>
[Experimental(SpecificationResources.DiagnosticId)]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SpecificationResourceAttribute : Attribute
{
    /// <summary>
    /// Gets the key of the group the resource's leases belong to, or null for a group of its own.
    /// </summary>
    /// <remarks>
    /// Resources with the same key share one <see cref="MaxConcurrency"/>, for example several resources that all
    /// use the same machine-wide port. A specification leasing from more than one limited group leases them in
    /// ordinal order of their keys, which is what rules out deadlocks between specifications.
    /// </remarks>
    public string? Key { get; init; }

    /// <summary>
    /// Gets the number of leases on the group that may be held at once: 0 for no limit, 1 for exclusive access.
    /// </summary>
    public int MaxConcurrency { get; init; }

    /// <summary>
    /// Gets how long stopping the resource at the end of the test session may take before it is reported as failed.
    /// </summary>
    public int StopTimeoutSeconds { get; init; } = 30;
}
