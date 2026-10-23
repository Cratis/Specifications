// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Holds the specification classes registered by the generated module initializers, per assembly.
/// </summary>
/// <remarks>
/// Registration happens before the entry point runs, so discovery is a list lookup: no assembly scanning and no reflection.
/// A test application runs the specifications of its own assembly; specifications compiled into referenced assemblies
/// are registered under those assemblies and are not run.
/// </remarks>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class SpecificationRegistry
{
    static readonly Dictionary<Assembly, List<SpecificationClassDescriptor>> _registered = [];
    static readonly Lock _lock = new();

    /// <summary>
    /// Registers a specification class.
    /// </summary>
    /// <param name="assembly">The assembly declaring the specification class.</param>
    /// <param name="specification">The descriptor of the specification class.</param>
    public static void Register(Assembly assembly, SpecificationClassDescriptor specification)
    {
        foreach (var fact in specification.Facts)
        {
            fact.Uid = $"{specification.Uid}.{fact.MethodName}";
        }

        lock (_lock)
        {
            if (!_registered.TryGetValue(assembly, out var classes)) _registered[assembly] = classes = [];
            classes.Add(specification);
        }
    }

    /// <summary>
    /// Gets the specification classes of the test application, ordered by their unique identifier.
    /// </summary>
    internal static IReadOnlyList<SpecificationClassDescriptor> Classes =>
        Assembly.GetEntryAssembly() is Assembly entry ? For(entry) : [];

    /// <summary>
    /// Gets the specification classes registered for an assembly, ordered by their unique identifier.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>The specification classes.</returns>
    internal static IReadOnlyList<SpecificationClassDescriptor> For(Assembly assembly)
    {
        SpecificationClassDescriptor[] classes;
        lock (_lock)
        {
            classes = _registered.TryGetValue(assembly, out var registered) ? [.. registered] : [];
        }

        Array.Sort(classes, static (left, right) => string.CompareOrdinal(left.Uid, right.Uid));
        return classes;
    }
}
