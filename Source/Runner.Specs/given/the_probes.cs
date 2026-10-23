// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Cratis.Specifications.Runner.given;

/// <summary>
/// Gives access to the probe specifications, registered by the generated module initializer of the probe assembly.
/// </summary>
internal static class the_probes
{
    public static IReadOnlyList<SpecificationClassDescriptor> All { get; } = Load();

    public static SpecificationClassDescriptor Get<TProbe>() =>
        All.Single(_ => _.Uid == typeof(TProbe).FullName);

    public static SpecificationWork WorkFor<TProbe>()
    {
        var specification = Get<TProbe>();
        return new(specification, specification.Facts);
    }

    public static FactResult ResultOf(SpecificationWork work, FactResult[] results, string method) =>
        results[work.Facts.ToList().FindIndex(_ => _.MethodName == method)];

    static IReadOnlyList<SpecificationClassDescriptor> Load()
    {
        var assembly = typeof(CallLog).Assembly;
        RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);
        return SpecificationRegistry.For(assembly);
    }
}
