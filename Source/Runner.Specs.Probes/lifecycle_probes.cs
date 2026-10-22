// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Probes;

public class a_recording_context : Specification
{
    protected virtual string Probe => GetType().Name;

    void Establish() => CallLog.Record(Probe, "Establish base");

    void Because() => CallLog.Record(Probe, "Because base");

    void Destroy() => CallLog.Record(Probe, "Destroy base");
}

public class synchronous_specification : a_recording_context, IDisposable
{
    void Given() => CallLog.Record(Probe, "Given");

    void When() => CallLog.Record(Probe, "When");

    void Destroy() => CallLog.Record(Probe, "Destroy");

    public void Dispose() => CallLog.Record(Probe, "Dispose");

    void should_be_the_first_fact() => CallLog.Record(Probe, "fact");

    public void should_be_the_second_fact() => CallLog.Record(Probe, "fact");

    [Fact] protected void third_fact_with_attribute() => CallLog.Record(Probe, "fact");
}

public class asynchronous_specification : a_recording_context, IAsyncDisposable
{
    async Task Establish()
    {
        await Task.Yield();
        CallLog.Record(Probe, "Establish");
    }

    async ValueTask Because()
    {
        await Task.Delay(1);
        CallLog.Record(Probe, "Because");
    }

    async Task Destroy()
    {
        await Task.Yield();
        CallLog.Record(Probe, "Destroy");
    }

    public ValueTask DisposeAsync()
    {
        CallLog.Record(Probe, "DisposeAsync");
        return ValueTask.CompletedTask;
    }

    async Task should_run_asynchronously()
    {
        await Task.Yield();
        CallLog.Record(Probe, "fact");
    }

    ValueTask should_run_as_a_value_task()
    {
        CallLog.Record(Probe, "fact");
        return ValueTask.CompletedTask;
    }
}
