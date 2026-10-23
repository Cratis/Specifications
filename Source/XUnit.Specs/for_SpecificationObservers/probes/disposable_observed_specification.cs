// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationObservers.probes;

sealed class disposable_observed_specification : Specification, IDisposable
{
    internal static bool disposed_before_scope_ended;

    void Destroy() { }

    public void Dispose() => disposed_before_scope_ended = true;

    [Fact] public void fact() { }
}
