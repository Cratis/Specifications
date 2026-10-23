// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationMethods.probes;

// None of these types derive from a test framework base class, so test discovery never finds them.
// They are run through SpecificationMethods only.
class probe_root
{
    internal readonly List<string> Calls = [];
}

static class LifecycleRun
{
    internal static async Task All<T>(T probe)
        where T : probe_root
    {
        await SpecificationMethods<T, probe_root>.Establish(probe);
        await SpecificationMethods<T, probe_root>.Because(probe);
        await SpecificationMethods<T, probe_root>.Destroy(probe);
    }
}

sealed class probe_without_lifecycle : probe_root
{
}

sealed class probe_with_private_lifecycle : probe_root
{
    void Establish() => Calls.Add("Establish");

    void Because() => Calls.Add("Because");

    void Destroy() => Calls.Add("Destroy");
}

class probe_base_with_private_lifecycle : probe_root
{
    void Establish() => Calls.Add("base.Establish");

    void Because() => Calls.Add("base.Because");

    void Destroy() => Calls.Add("base.Destroy");
}

sealed class probe_derived_with_private_lifecycle : probe_base_with_private_lifecycle
{
    void Establish() => Calls.Add("derived.Establish");

    void Because() => Calls.Add("derived.Because");

    void Destroy() => Calls.Add("derived.Destroy");
}

class probe_base_with_protected_lifecycle : probe_root
{
    protected void Establish() => Calls.Add("base.Establish");

    protected void Because() => Calls.Add("base.Because");

    protected void Destroy() => Calls.Add("base.Destroy");
}

sealed class probe_derived_from_protected_lifecycle_without_its_own : probe_base_with_protected_lifecycle
{
}

class probe_base_with_virtual_lifecycle : probe_root
{
    protected virtual void Establish() => Calls.Add("base.Establish");
}

sealed class probe_overriding_virtual_lifecycle : probe_base_with_virtual_lifecycle
{
    protected override void Establish() => Calls.Add("derived.Establish");
}

sealed class probe_with_overloaded_lifecycle : probe_root
{
    void Establish() => Calls.Add("Establish");

    void Establish(int ignored) => Calls.Add("Establish(int)");
}

sealed class probe_with_async_lifecycle : probe_root
{
    async Task Establish()
    {
        Calls.Add("Establish.started");
        await Task.Delay(20);
        Calls.Add("Establish.completed");
    }

    async ValueTask Because()
    {
        Calls.Add("Because.started");
        await Task.Delay(20);
        Calls.Add("Because.completed");
    }

    async ValueTask<int> Destroy()
    {
        Calls.Add("Destroy.started");
        await Task.Delay(20);
        Calls.Add("Destroy.completed");
        return 42;
    }
}

sealed class probe_with_throwing_establish : probe_root
{
    void Establish() => throw new InvalidOperationException("deliberate Establish failure");
}

sealed class probe_with_async_throwing_because : probe_root
{
    async Task Because()
    {
        await Task.Yield();
        throw new InvalidOperationException("deliberate Because failure");
    }
}

sealed class probe_with_cancelling_establish : probe_root
{
    void Establish() => throw new OperationCanceledException();
}

sealed class probe_with_async_cancelling_because : probe_root
{
    async Task Because()
    {
        await Task.Yield();
        throw new TaskCanceledException();
    }
}

class probe_with_aliases : probe_root
{
    void Given() => Calls.Add("Given");
    void When() => Calls.Add("When");
    void Destroy() => Calls.Add("Destroy");
}

sealed class probe_with_mixed_aliases : probe_with_aliases
{
    void Establish() => Calls.Add("derived.Establish");
}

sealed class probe_with_ambiguous_aliases : probe_root
{
    void Given() { }
    void Establish() { }
}
