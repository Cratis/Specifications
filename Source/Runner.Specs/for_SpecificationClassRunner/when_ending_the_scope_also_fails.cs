// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_ending_the_scope_also_fails : Specification
{
    SpecificationClassResult result;
    failing_scope observer;

    async Task Because()
    {
        var probe = given.the_probes.Get<failing_fact_and_teardown>();
        var descriptor = new SpecificationClassDescriptor(probe.Uid, probe.Namespace, probe.TypeName,
            nameof(when_ending_the_scope_also_fails), probe.Create, probe.Establish, probe.Because, probe.Destroy, probe.Facts);
        observer = new();
        SpecificationObservers.Add(observer);
        try
        {
            result = await SpecificationClassRunner.Run(new(descriptor, descriptor.Facts), sequentialFacts: true);
        }
        finally
        {
            SpecificationObservers.Remove(observer);
        }
    }

    void should_attempt_scope_cleanup_once_despite_the_other_teardown_errors() => observer.Calls.ShouldEqual(1);
    void should_preserve_all_three_errors_in_one_teardown_result() => ((AggregateException)result.TearDownFailure.Value.Exception).Flatten().InnerExceptions.Select(_ => _.Message).ShouldEqualInAnyOrder("Destroy failed", "Dispose failed", "Scope failed");
    void should_keep_the_fact_failure_separate() => result.Facts.Single(_ => _.Outcome == FactOutcome.Failed).Exception.Message.ShouldEqual("Fact failed");
    void should_keep_the_passing_fact() => result.Facts.Count(_ => _.Outcome == FactOutcome.Passed).ShouldEqual(1);

    sealed class failing_scope : ISpecificationObserver
    {
        public int Calls { get; private set; }

        public ValueTask ScopeStarting(SpecificationScope scope) => ValueTask.CompletedTask;

        public void PhaseCompleted(SpecificationScope scope, SpecificationPhase phase, long startTimestamp, long endTimestamp, Exception failure) { }

        public ValueTask ScopeEnded(SpecificationScope scope)
        {
            if (scope.DisplayName != nameof(when_ending_the_scope_also_fails)) return ValueTask.CompletedTask;
            Calls++;
            return ValueTask.FromException(new InvalidOperationException("Scope failed"));
        }
    }
}
