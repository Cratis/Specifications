// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

sealed class disposable_specification_with_throwing_destroy : Specification, IDisposable
{
    internal static int dispose_count;
    void Destroy() => throw new InvalidOperationException("deliberate Destroy failure");
    public void Dispose() { dispose_count++; GC.SuppressFinalize(this); }
    [Fact] public void first_fact() { }
}
