// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// TreeNodeFilter has no public constructor in the pinned MTP API.
#pragma warning disable TPEXP

using System.Runtime.CompilerServices;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;

namespace Cratis.Specifications.Runner.for_MessageBusResultSink;

public class when_teardown_fails_after_filtering : Specification
{
    readonly given.a_recording_message_bus bus = new();
    SpecificationWork selected;
    SpecificationClassResult result;
    TestNode stable_node;

    IEnumerable<TestNode> FailedNodes => bus.Nodes.Where(_ => _.Properties.OfType<FailedTestNodeStateProperty>().Length > 0);

    async Task Because()
    {
        var descriptor = given.the_probes.Get<failing_fact_and_teardown>();
        var work = new SpecificationWork(descriptor, descriptor.Facts);
        var filter = Filter($"/Probes/{work.Specification.Namespace}/{work.Specification.TypeName}/should_pass");
        selected = WorkSelector.Select([work.Specification], filter, "Probes").Single();
        var trx = new TrxCapability();
        var producer = new SpecificationFramework(SpecificationRunOptions.Default, trx, publishInProgress: true);
        var sink = new MessageBusResultSink(producer, bus, new SessionUid("session"), new TestNodeFactory("Probes", trx), publishInProgress: true);
        result = await SpecificationClassRunner.Run(selected, sequentialFacts: true);
        await sink.Starting(selected);
        await sink.Completed(selected, result.Facts, result.TearDownFailure);
        stable_node = new TestNodeFactory("Probes", new TrxCapability()).CreateTearDown(work.Specification, result.TearDownFailure.Value);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    static extern TreeNodeFilter Filter(string expression);

    void should_run_only_the_selected_fact() => selected.Facts.Single().MethodName.ShouldEqual("should_pass");
    void should_publish_the_passing_fact() => bus.Nodes.Count(_ => _.Properties.OfType<PassedTestNodeStateProperty>().Length > 0).ShouldEqual(1);
    void should_publish_exactly_one_separate_failure() => FailedNodes.Count().ShouldEqual(1);
    void should_identify_the_class_teardown_clearly() => FailedNodes.Single().DisplayName.ShouldEqual(selected.Specification.DisplayName + " (teardown)");
    void should_keep_the_teardown_uid_stable() => FailedNodes.Single().Uid.ShouldEqual(stable_node.Uid);
    void should_not_reuse_the_fact_uid() => FailedNodes.Single().Uid.Value.ShouldNotEqual(selected.Facts.Single().Uid);
    void should_carry_the_aggregated_exception() => FailedNodes.Single().Properties.Single<FailedTestNodeStateProperty>().Exception.ShouldEqual(result.TearDownFailure.Value.Exception);
    void should_preserve_the_fact_outcome_in_the_report() => ClassReport.From(selected, result.Facts, result.TearDownFailure).Facts[0].Outcome.ShouldEqual(FactOutcome.Passed);
    void should_include_teardown_in_the_report() => ClassReport.From(selected, result.Facts, result.TearDownFailure).Facts[1].Outcome.ShouldEqual(FactOutcome.Failed);
}
