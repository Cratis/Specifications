// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Probes;

public class failing_fact_and_teardown : Specification, IDisposable
{
    void Destroy() => throw new InvalidOperationException("Destroy failed");

    public void Dispose() => throw new InvalidOperationException("Dispose failed");

    void should_pass() { }

    void should_fail() => throw new InvalidOperationException("Fact failed");
}
