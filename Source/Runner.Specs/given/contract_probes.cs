// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Cratis.Specifications.Runner.given;

internal static class contract_probes
{
    public static SpecificationWork WorkFor<T>()
    {
        var assembly = typeof(T).Assembly;
        RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);
        var descriptor = SpecificationRegistry.For(assembly).Single(_ => _.Uid == typeof(T).FullName);
        return new(descriptor, descriptor.Facts);
    }
}
