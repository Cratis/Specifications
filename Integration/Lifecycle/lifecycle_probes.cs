// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;

namespace LifecycleProbes;

public abstract class lifecycle_context : Specification, IDisposable
{
    static readonly Lock Gate = new();

    protected void Record(string operation)
    {
        var path = Environment.GetEnvironmentVariable("CRATIS_LIFECYCLE_TRACE");
        if (path is null) return;
        lock (Gate) File.AppendAllText(path, $"{GetType().Name}:{operation}{Environment.NewLine}");
    }

    void Establish() => Record("Establish");
    void Because() => Record("Because");
    void Destroy() => Record("Destroy");
    public void Dispose() => Record("Dispose");

#if XUNIT
    [Xunit.Fact]
#elif NUNIT
    [NUnit.Framework.Test]
#elif TUNIT
    [global::TUnit.Core.Test]
#endif
    public void should_run_first() => Record("fact1");

#if XUNIT
    [Xunit.Fact]
#elif NUNIT
    [NUnit.Framework.Test]
#elif TUNIT
    [global::TUnit.Core.Test]
#endif
    public void should_run_second() => Record("fact2");
}
