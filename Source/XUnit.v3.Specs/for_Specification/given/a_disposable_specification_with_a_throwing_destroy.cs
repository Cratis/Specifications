// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.given;

public class a_disposable_specification_with_a_throwing_destroy : Specification, IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose() => Disposed = true;

    void Destroy() => throw new InvalidOperationException("destroy failed");
}
