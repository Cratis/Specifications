// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationObservers.given;

internal sealed class disposable_instance : IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose() => Disposed = true;
}
