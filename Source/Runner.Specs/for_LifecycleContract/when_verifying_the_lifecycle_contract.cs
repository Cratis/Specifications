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
// Fact + teardown failure | preserve fact, Destroy and disposal errors
// Disposal                | IAsyncDisposable preferred over IDisposable; once per class
// All skipped             | no construction, lifecycle or disposal

public class when_verifying_the_lifecycle_contract : Specification
{
    FactResult[] overridden;
    FactResult[] hidden;
    FactResult[] establish_failure;
    FactResult[] teardown_failure;
    FactResult[] because_failure;
    FactResult[] duplicate_alias;
    FactResult[] skipped;
    SpecificationWork teardown;

    void Establish()
    {
        lifecycle_context<overridden_context>.Calls.Clear();
        lifecycle_context<hidden_context>.Calls.Clear();
        lifecycle_context<failing_establish>.Calls.Clear();
        lifecycle_context<failing_teardown>.Calls.Clear();
        lifecycle_context<failing_async_because>.Calls.Clear();
        all_skipped.Calls = 0;
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
    }

    static Task<FactResult[]> Run<T>() => SpecificationClassRunner.Run(given.contract_probes.WorkFor<T>(), sequentialFacts: true);

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

    void should_preserve_all_failures_from_fact_destroy_and_disposal()
    {
        var result = given.the_probes.ResultOf(teardown, teardown_failure, "should_fail");
        var errors = ((AggregateException)result.Exception).Flatten().InnerExceptions;
        errors.Count.ShouldEqual(3);
        errors.ShouldContain(error => error.Message == "Fact failed");
        errors.ShouldContain(failing_teardown.DestroyFailure);
        errors.ShouldContain(failing_teardown.DisposeFailure);
    }

    void should_fail_a_previously_passing_fact_after_teardown_fails() => given.the_probes.ResultOf(teardown, teardown_failure, "should_pass").Outcome.ShouldEqual(FactOutcome.Failed);

    void should_preserve_both_teardown_failures_for_a_previously_passing_fact()
    {
        var result = given.the_probes.ResultOf(teardown, teardown_failure, "should_pass");
        var errors = ((AggregateException)result.Exception).Flatten().InnerExceptions;
        errors.ShouldEqualInAnyOrder(failing_teardown.DestroyFailure, failing_teardown.DisposeFailure);
    }

    void should_reject_because_and_when_together() => duplicate_alias.Single().Exception.Message.ShouldContain("declares both Because and When");

    void should_not_construct_or_run_an_all_skipped_class() => all_skipped.Calls.ShouldEqual(0);

    void should_preserve_the_skip_reason() => skipped.Single().SkipReason.ShouldEqual("By design");
}
