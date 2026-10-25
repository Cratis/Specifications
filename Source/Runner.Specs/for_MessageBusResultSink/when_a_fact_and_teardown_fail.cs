// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.TestHost;

namespace Cratis.Specifications.Runner.for_MessageBusResultSink;

public class when_a_fact_and_teardown_fail : Specification
{
    readonly given.a_recording_message_bus bus = new();
    SpecificationWork work;
    ClassReport report;

    IEnumerable<TestNode> FailedNodes => bus.Nodes.Where(_ => _.Properties.OfType<FailedTestNodeStateProperty>().Length > 0);
    TestNode Fact => FailedNodes.Single(_ => _.Uid == new TestNodeUid(work.Facts.Single(_ => _.MethodName == "should_fail").Uid));
    TestNode TearDown => FailedNodes.Single(_ => _.DisplayName.EndsWith(" (teardown)", StringComparison.Ordinal));

    async Task Because()
    {
        var descriptor = given.the_probes.Get<failing_fact_and_teardown>();
        work = new(descriptor, descriptor.Facts);
        var trx = new TrxCapability();
        var producer = new SpecificationFramework(SpecificationRunOptions.Default, trx, publishInProgress: true);
        var sink = new MessageBusResultSink(producer, bus, new SessionUid("session"), new TestNodeFactory("Probes", trx), publishInProgress: true);
        var result = await SpecificationClassRunner.Run(work, sequentialFacts: true);
        await sink.Completed(work, result.Facts, result.TearDownFailure);
        report = ClassReport.From(work, result.Facts, result.TearDownFailure);
    }

    void should_publish_two_distinct_failures() => FailedNodes.Count().ShouldEqual(2);
    void should_keep_the_passing_fact() => bus.Nodes.Count(_ => _.Properties.OfType<PassedTestNodeStateProperty>().Length > 0).ShouldEqual(1);
    void should_not_merge_teardown_with_the_fact_error() => Fact.Properties.Single<FailedTestNodeStateProperty>().Exception.GetType().ShouldEqual(typeof(InvalidOperationException));
    void should_keep_the_fact_error() => Fact.Properties.Single<FailedTestNodeStateProperty>().Exception.Message.ShouldEqual("Fact failed");
    void should_aggregate_only_the_teardown_errors() => ((AggregateException)TearDown.Properties.Single<FailedTestNodeStateProperty>().Exception).Flatten().InnerExceptions.Select(_ => _.Message).ShouldEqual("Destroy failed", "Dispose failed");
    void should_keep_both_failures_in_the_report() => report.Facts.Count(_ => _.Outcome == FactOutcome.Failed).ShouldEqual(2);
    void should_not_report_teardown_as_a_setup_failure() => report.ContextFailure.ShouldBeNull();
}
