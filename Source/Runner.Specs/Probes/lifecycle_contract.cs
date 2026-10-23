// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.ContractProbes;

public abstract class lifecycle_context<T> : Specification
{
    public static List<string> Calls { get; } = [];

    async ValueTask Establish()
    {
        await Task.Yield();
        Calls.Add("Establish base");
    }

    protected virtual void Because() => Calls.Add("Because base");

    void Destroy() => Calls.Add("Destroy base");
}

public class overridden_context : lifecycle_context<overridden_context>, IDisposable, IAsyncDisposable
{
    public overridden_context() => Calls.Add("Create");

    void Given() => Calls.Add("Given");

    protected override void Because() => Calls.Add("Because override");

    void Destroy() => Calls.Add("Destroy");

    public void Dispose() => Calls.Add("Dispose");

    public async ValueTask DisposeAsync()
    {
        await Task.Yield();
        Calls.Add("DisposeAsync");
    }

    void should_run() => Calls.Add("fact");
}

public class hidden_context : lifecycle_context<hidden_context>
{
    protected new void Because() => Calls.Add("Because hidden");

    void should_run() => Calls.Add("fact");
}

public class failing_establish : lifecycle_context<failing_establish>, IDisposable
{
    public static readonly InvalidOperationException Failure = new("Establish failed");

    void Given() => throw Failure;

    public void Dispose() => Calls.Add("Dispose");

    void should_not_run() => Calls.Add("fact");
}

public class failing_async_because : lifecycle_context<failing_async_because>, IDisposable
{
    public static readonly InvalidOperationException Failure = new("Async Because failed");

    async Task When()
    {
        await Task.Yield();
        throw Failure;
    }

    public void Dispose() => Calls.Add("Dispose");

    void should_not_run() => Calls.Add("fact");
}

public class failing_teardown : lifecycle_context<failing_teardown>, IAsyncDisposable
{
    public static readonly InvalidOperationException DestroyFailure = new("Destroy failed");
    public static readonly InvalidOperationException DisposeFailure = new("Dispose failed");

    void Destroy() => throw DestroyFailure;

    public async ValueTask DisposeAsync()
    {
        await Task.Yield();
        throw DisposeFailure;
    }

    void should_fail() => throw new InvalidOperationException("Fact failed");

    void should_pass() { }
}

public class when_and_because : Specification
{
    void When() { }

    void Because() { }

    void should_not_run() { }
}

public class all_skipped : Specification, IDisposable
{
    public static int Calls { get; set; }

    public all_skipped() => Calls++;

    void Given() => Calls++;

    void When() => Calls++;

    void Destroy() => Calls++;

    public void Dispose() => Calls++;

    [Fact(Skip = "By design")] void should_not_run() => Calls++;
}

public class failing_sync_because : lifecycle_context<failing_sync_because>, IDisposable
{
    public static readonly InvalidOperationException Failure = new("Synchronous Because failed");

    void When() => throw Failure;

    public void Dispose() => Calls.Add("Dispose");

    void should_not_run() => Calls.Add("fact");
}

public class cancelling_because : lifecycle_context<cancelling_because>, IDisposable
{
    public static CancellationTokenSource Cancellation { get; set; }

    async Task When()
    {
        await Cancellation.CancelAsync();
        Calls.Add("When");
    }

    public void Dispose() => Calls.Add("Dispose");

    void should_run() => Calls.Add("fact");
}

public class queued_behind_a_stop : lifecycle_context<queued_behind_a_stop>, IDisposable
{
    public queued_behind_a_stop() => Calls.Add("Create");

    public void Dispose() => Calls.Add("Dispose");

    void should_not_run() => Calls.Add("fact");
}

public class partly_skipped : lifecycle_context<partly_skipped>, IDisposable
{
    public void Dispose() => Calls.Add("Dispose");

    void should_run() => Calls.Add("fact");

    [Fact(Skip = "Not now")] void should_not_run() => Calls.Add("skipped fact");
}
