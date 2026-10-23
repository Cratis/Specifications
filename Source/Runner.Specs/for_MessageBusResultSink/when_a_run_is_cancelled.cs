// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.TestHost;

namespace Cratis.Specifications.Runner.for_MessageBusResultSink;

public class when_a_run_is_cancelled : Specification
{
    readonly recording_bus bus = new();
    IReadOnlyList<SpecificationWork> work;

    async Task Because()
    {
        using var cancellation = new CancellationTokenSource();
        work = given.specification_work.Many(5, _ =>
        {
            cancellation.Cancel();
            return ValueTask.CompletedTask;
        });
        var trx = new TrxCapability();
        var producer = new SpecificationFramework(SpecificationRunOptions.Default, trx);
        var sink = new MessageBusResultSink(producer, bus, new SessionUid("cancelled"), new TestNodeFactory("Probes", trx), publishInProgress: true);
        var remaining = await SpecificationScheduler.Run(work,
            SpecificationRunOptions.Default with { MaxParallelClasses = 1 }, sink, RunCache.Empty, cancellation.Token);
        foreach (var item in remaining)
        {
            foreach (var fact in item.Facts) await sink.NotRun(item.Specification, fact, "The run was cancelled.");
        }
    }

    void should_report_each_selected_fact_once_in_a_terminal_state() => bus.Nodes
        .Where(_ => !_.Properties.OfType<InProgressTestNodeStateProperty>().Any())
        .Select(_ => _.Uid.Value).ShouldEqual(work.SelectMany(_ => _.Facts).Select(_ => _.Uid));

    void should_complete_the_running_fact() => bus.Nodes.Count(_ => _.Properties.OfType<PassedTestNodeStateProperty>().Any()).ShouldEqual(1);

    void should_skip_every_queued_fact() => bus.Nodes.Count(_ => _.Properties.OfType<SkippedTestNodeStateProperty>().Any()).ShouldEqual(4);

    sealed class recording_bus : IMessageBus
    {
        public List<TestNode> Nodes { get; } = [];

        public Task PublishAsync(IDataProducer dataProducer, IData data)
        {
            Nodes.Add(((TestNodeUpdateMessage)data).TestNode);
            return Task.CompletedTask;
        }
    }
}
