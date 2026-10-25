// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.ContractProbes;

namespace Cratis.Specifications.Runner.for_LifecycleContract;

// Shared lifecycle cases (mirror these in the core lifecycle specifications):
// Case                    | Contract
// Given/When              | aliases of Establish/Because; both names in one class are an error
// Private generic hooks   | awaited, base first; Task/ValueTask/void are supported
// Override / hidden hook  | virtual override once / both distinct declarations base first
// Failed Establish        | no Because/facts; Destroy and disposal still run once
// Fact + teardown failure | preserve fact outcome; aggregate Destroy and disposal in a separate class result
// Disposal                | IAsyncDisposable preferred over IDisposable; once per class
// All skipped             | no construction, lifecycle or disposal
// Partly skipped          | skipped facts never run; lifecycle and disposal run once for the rest
// Synchronous act failure | fails the facts with the original exception; Destroy and disposal still run
// Cancellation            | the running class finishes Destroy and disposal; queued classes are never constructed
// Stop (fail fast)        | the failing class finishes Destroy and disposal; queued classes are never constructed

public class when_verifying_the_lifecycle_contract : Specification
{
    FactResult[] overridden;
    FactResult[] hidden;
    FactResult[] establish_failure;
    SpecificationClassResult teardown_failure;
    FactResult[] because_failure;
    FactResult[] duplicate_alias;
    FactResult[] skipped;
    FactResult[] sync_because_failure;
    FactResult[] partly_skipped_results;
    IReadOnlyList<SpecificationWork> not_started_after_cancellation;
    IReadOnlyList<SpecificationWork> not_started_after_stop;
    given.a_recording_sink cancellation_sink;
    IReadOnlyList<string> queued_calls_after_cancellation;
    SpecificationWork teardown;

    void Establish()
    {
        lifecycle_context<overridden_context>.Calls.Clear();
        lifecycle_context<hidden_context>.Calls.Clear();
        lifecycle_context<failing_establish>.Calls.Clear();
        lifecycle_context<failing_teardown>.Calls.Clear();
        lifecycle_context<failing_async_because>.Calls.Clear();
        all_skipped.Calls = 0;
        lifecycle_context<failing_sync_because>.Calls.Clear();
        lifecycle_context<cancelling_because>.Calls.Clear();
        lifecycle_context<queued_behind_a_stop>.Calls.Clear();
        lifecycle_context<partly_skipped>.Calls.Clear();
    }

    async Task Because()
    {
        overridden = await Run<overridden_context>();
        hidden = await Run<hidden_context>();
        establish_failure = await Run<failing_establish>();
        because_failure = await Run<failing_async_because>();
        teardown = given.contract_probes.WorkFor<failing_teardown>();
        teardown_failure = await SpecificationClassRunner.Run(teardown, sequentialFacts: true);
        duplicate_alias = await Run<when_and_because>();
        skipped = await Run<all_skipped>();
        partly_skipped_results = await Run<partly_skipped>();
        sync_because_failure = await Run<failing_sync_because>();
        await RunCancelled();
        await RunStopped();
    }

    async Task RunCancelled()
    {
        using var cancellation = new CancellationTokenSource();
        cancelling_because.Cancellation = cancellation;
        cancellation_sink = new();
        not_started_after_cancellation = await SpecificationScheduler.Run(
            [given.contract_probes.WorkFor<cancelling_because>(), given.contract_probes.WorkFor<queued_behind_a_stop>()],
            SpecificationRunOptions.Default with { MaxParallelClasses = 1, SequentialFacts = true }, cancellation_sink, RunCache.Empty, cancellation.Token);
        queued_calls_after_cancellation = [.. lifecycle_context<queued_behind_a_stop>.Calls];
        lifecycle_context<queued_behind_a_stop>.Calls.Clear();
    }

    async Task RunStopped()
    {
        lifecycle_context<failing_teardown>.Calls.Clear();
        not_started_after_stop = await SpecificationScheduler.Run(
            [given.contract_probes.WorkFor<failing_teardown>(), given.contract_probes.WorkFor<queued_behind_a_stop>()],
            SpecificationRunOptions.Default with { MaxParallelClasses = 1, SequentialFacts = true, FailFast = true }, new given.a_recording_sink(), RunCache.Empty, CancellationToken.None);
    }

    static async Task<FactResult[]> Run<T>() => (await SpecificationClassRunner.Run(given.contract_probes.WorkFor<T>(), sequentialFacts: true)).Facts;

    void should_run_private_generic_hooks_and_dispatch_an_override_once() => lifecycle_context<overridden_context>.Calls.ShouldEqual(
        ["Create", "Establish base", "Given", "Because override", "fact", "Destroy base", "Destroy", "DisposeAsync"]);

    void should_prefer_async_disposal_and_pass_the_fact() => overridden.Single().Outcome.ShouldEqual(FactOutcome.Passed);

    void should_run_both_a_base_hook_and_a_hidden_hook() => lifecycle_context<hidden_context>.Calls.ShouldEqual(
        ["Establish base", "Because base", "Because hidden", "fact", "Destroy base"]);

    void should_pass_the_hidden_context() => hidden.Single().Outcome.ShouldEqual(FactOutcome.Passed);

    void should_skip_because_and_facts_but_still_destroy_and_dispose_after_failed_establish() => lifecycle_context<failing_establish>.Calls.ShouldEqual(
        ["Establish base", "Destroy base", "Dispose"]);

    void should_preserve_the_original_establish_exception() => establish_failure.Single().Exception.ShouldEqual(failing_establish.Failure);

    void should_destroy_and_dispose_after_asynchronous_because_failure() => lifecycle_context<failing_async_because>.Calls.ShouldEqual(
        ["Establish base", "Because base", "Destroy base", "Dispose"]);

    void should_preserve_the_original_asynchronous_because_exception() => because_failure.Single().Exception.ShouldEqual(failing_async_because.Failure);

    void should_keep_the_fact_failure_separate_from_teardown()
    {
        var result = given.the_probes.ResultOf(teardown, teardown_failure.Facts, "should_fail");
        result.Outcome.ShouldEqual(FactOutcome.Failed);
        result.Exception.ShouldBeOfExactType<InvalidOperationException>();
        result.Exception.Message.ShouldEqual("Fact failed");
    }

    void should_keep_a_previously_passing_fact_after_teardown_fails() => given.the_probes.ResultOf(teardown, teardown_failure.Facts, "should_pass").Outcome.ShouldEqual(FactOutcome.Passed);

    void should_not_attach_teardown_to_the_passing_fact() => given.the_probes.ResultOf(teardown, teardown_failure.Facts, "should_pass").Exception.ShouldBeNull();

    void should_report_both_teardown_failures_as_one_separate_failure()
    {
        var result = teardown_failure.TearDownFailure.Value;
        result.Outcome.ShouldEqual(FactOutcome.Failed);
        var errors = ((AggregateException)result.Exception).Flatten().InnerExceptions;
        errors.ShouldEqualInAnyOrder(failing_teardown.DestroyFailure, failing_teardown.DisposeFailure);
    }

    void should_reject_because_and_when_together() => duplicate_alias.Single().Exception.Message.ShouldContain("declares both Because and When");

    void should_not_construct_or_run_an_all_skipped_class() => all_skipped.Calls.ShouldEqual(0);

    void should_preserve_the_skip_reason() => skipped.Single().SkipReason.ShouldEqual("By design");

    void should_never_run_a_skipped_fact_of_a_partly_skipped_class() => lifecycle_context<partly_skipped>.Calls.ShouldEqual(["Establish base", "Because base", "fact", "Destroy base", "Dispose"]);

    void should_report_the_skipped_fact_of_a_partly_skipped_class() => partly_skipped_results.Select(_ => _.Outcome).ShouldEqual(FactOutcome.Passed, FactOutcome.Skipped);

    void should_destroy_and_dispose_after_a_synchronous_because_failure() => lifecycle_context<failing_sync_because>.Calls.ShouldEqual(["Establish base", "Because base", "Destroy base", "Dispose"]);

    void should_preserve_the_original_synchronous_because_exception() => sync_because_failure.Single().Exception.ShouldEqual(failing_sync_because.Failure);

    void should_not_construct_a_class_queued_behind_a_cancellation() => queued_calls_after_cancellation.ShouldBeEmpty();

    void should_report_the_class_queued_behind_a_cancellation_as_not_started() => not_started_after_cancellation.Single().Specification.Uid.ShouldEqual(typeof(queued_behind_a_stop).FullName);

    void should_finish_destroy_and_disposal_of_the_class_running_during_a_cancellation()
    {
        var calls = lifecycle_context<cancelling_because>.Calls;
        calls.Skip(calls.Count - 2).ShouldEqual("Destroy base", "Dispose");
    }

    void should_complete_the_class_running_during_a_cancellation_once() => cancellation_sink.Completed.Count.ShouldEqual(1);

    void should_not_construct_a_class_queued_behind_a_stop() => lifecycle_context<queued_behind_a_stop>.Calls.ShouldBeEmpty();

    void should_report_the_class_queued_behind_a_stop_as_not_started() => not_started_after_stop.Single().Specification.Uid.ShouldEqual(typeof(queued_behind_a_stop).FullName);

    void should_finish_destroy_and_disposal_of_the_class_that_stopped_the_run() => lifecycle_context<failing_teardown>.Calls.ShouldEqual(["Establish base", "Because base", "Destroy base"]);
}
