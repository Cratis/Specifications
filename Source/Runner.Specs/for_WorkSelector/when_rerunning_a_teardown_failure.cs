// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;

namespace Cratis.Specifications.Runner.for_WorkSelector;

public class when_rerunning_a_teardown_failure : Specification
{
    readonly SpecificationClassDescriptor specification = given.the_probes.Get<failing_fact_and_teardown>();
    readonly given.a_recording_message_bus bus = new();
    SpecificationWork selected;
    SpecificationClassResult result;
    TestNode reported;

    async Task Because()
    {
        var nodes = new TestNodeFactory("Probes", new TrxCapability());
        var initial = await SpecificationClassRunner.Run(new(specification, [specification.Facts.Single(_ => _.MethodName == "should_pass")]), sequentialFacts: true);
        reported = nodes.CreateTearDown(specification, initial.TearDownFailure.Value);
        var filter = new TestNodeUidListFilter([reported.Uid]);
        selected = WorkSelector.Select(given.the_probes.All, filter).Single();
        result = await SpecificationClassRunner.Run(selected, sequentialFacts: true);
        var producer = new SpecificationFramework(SpecificationRunOptions.Default, new TrxCapability(), publishInProgress: true);
        var sink = new MessageBusResultSink(producer, bus, new SessionUid("rerun"), nodes, publishInProgress: true);
        await sink.Completed(selected, result.Facts, result.TearDownFailure);
    }

    void should_select_only_the_owning_class() => selected.Specification.ShouldEqual(specification);
    void should_select_all_its_facts_not_just_the_initially_selected_fact() => selected.Facts.Select(_ => _.Uid).ShouldEqual(specification.Facts.Select(_ => _.Uid));
    void should_run_the_passing_fact() => result.Facts.Count(_ => _.Outcome == FactOutcome.Passed).ShouldEqual(1);
    void should_run_the_failing_fact() => result.Facts.Single(_ => _.Outcome == FactOutcome.Failed).Exception.Message.ShouldEqual("Fact failed");
    void should_publish_all_facts_and_the_teardown() => bus.Nodes.Count.ShouldEqual(specification.Facts.Count + 1);
    void should_report_the_teardown_failure_again() => bus.Nodes.Single(_ => _.Uid == reported.Uid).Properties.Single<FailedTestNodeStateProperty>().Exception.Message.ShouldEqual(result.TearDownFailure.Value.Exception.Message);
    void should_keep_both_cleanup_errors() => ((AggregateException)result.TearDownFailure.Value.Exception).Flatten().InnerExceptions.Select(_ => _.Message).ShouldEqual("Destroy failed", "Dispose failed");
    void should_not_select_the_class_twice_when_a_fact_uid_is_also_requested() => WorkSelector.Select(given.the_probes.All, new TestNodeUidListFilter([reported.Uid, new TestNodeUid(specification.Facts[0].Uid)])).Count.ShouldEqual(1);
}
