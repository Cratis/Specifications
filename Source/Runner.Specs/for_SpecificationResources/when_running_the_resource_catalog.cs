// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Resources;

namespace Cratis.Specifications.Runner.for_SpecificationResources;

public class when_running_the_resource_catalog : Specification
{
    ResourceCatalogResults results;

    async Task Because() => results = await ResourceCatalog.Run();

    void should_start_a_resource_once_for_parallel_acquirers() => results.StartsForParallelAcquirers.ShouldEqual(1);
    void should_create_a_resource_once_for_parallel_acquirers() => results.CreatedForParallelAcquirers.ShouldEqual(1);
    void should_give_parallel_acquirers_the_same_resource() => results.InstancesForParallelAcquirers.ShouldEqual(1);
    void should_register_the_stop_with_the_session_once() => results.SessionEndRegistrations.ShouldEqual(1);
    void should_create_nothing_that_is_never_acquired() => results.CreatedWhenNeverAcquired.ShouldEqual(0);
    void should_not_register_with_the_session_when_nothing_is_acquired() => results.SessionEndRegistrationsWhenNeverAcquired.ShouldEqual(0);
    void should_fail_the_first_acquirer_with_the_start_failure() => results.FirstAcquirerFailure.ShouldBeSame(results.StartFailure);
    void should_fail_later_acquirers_with_the_same_start_failure() => results.SecondAcquirerFailure.ShouldBeSame(results.StartFailure);
    void should_not_start_a_failed_resource_again() => results.StartsAfterFailure.ShouldEqual(1);
    void should_dispose_a_resource_whose_start_failed() => results.DisposalsAfterFailedStart.ShouldEqual(1);
    void should_report_a_failed_partial_start_cleanup_when_the_session_ends() => results.ReportedPartialStartCleanupFailure.InnerException.ShouldBeSame(results.PartialStartCleanupFailure);
    void should_release_leases_after_the_scope_in_reverse_order() => results.ReleaseSequence.ShouldEqual<string>(ResourceCatalogResults.ReleaseOrder);
    void should_give_the_lease_the_specification_display_name() => results.LeaseDisplayName.ShouldEqual("releases in reverse");
    void should_give_every_lease_its_own_id() => results.LeaseIdsAreUnique.ShouldBeTrue();
    void should_hold_one_exclusive_lease_at_a_time() => results.PeakForExclusiveGroup.ShouldEqual(1);
    void should_lease_independent_groups_together() => results.IndependentGroupsLeasedTogether.ShouldBeTrue();
    void should_hold_leases_on_independent_groups_at_once() => results.PeakForIndependentGroups.ShouldEqual(2);
    void should_reject_an_out_of_order_lease() => results.OutOfOrderFailure.ShouldBeOfExactType<SpecificationResourceOrderException>();
    void should_not_hold_the_group_of_a_rejected_lease() => results.OutOfOrderLeaseLeftGroupFree.ShouldBeTrue();
    void should_fail_a_cancelled_wait_for_a_lease() => results.CancelledWaitingForLease.ShouldBeOfExactType<SpecificationCancelledException>();
    void should_fail_a_cancelled_wait_for_a_start() => results.CancelledWaitingForStart.ShouldBeOfExactType<SpecificationCancelledException>();
    void should_carry_on_starting_after_a_cancelled_wait() => results.StartCarriedOnAfterCancelledWait.ShouldBeTrue();
    void should_report_every_stop_failure() => ((AggregateException)results.ReportedStopFailures).InnerExceptions.Select(_ => _.GetType()).ShouldEqual<Type>(typeof(TimeoutException), typeof(InvalidOperationException));
    void should_report_the_stop_failure_itself() => ((AggregateException)results.ReportedStopFailures).InnerExceptions[1].InnerException.ShouldBeSame(results.StopFailure);
    void should_stop_resources_in_reverse_start_order() => results.StopSequence.ShouldEqual<string>("stop quick", "stop unlimited");
    void should_return_a_borrowed_resource() => results.BorrowedResourceReturned.ShouldBeTrue();
    void should_never_start_a_borrowed_resource() => results.BorrowedStarts.ShouldEqual(0);
    void should_never_dispose_a_borrowed_resource() => results.BorrowedDisposals.ShouldEqual(0);
    void should_fail_the_specification_whose_lease_failed() => results.FailedLeaseEstablishFailure.ShouldBeSame(results.LeaseFailure);
    void should_free_the_group_when_a_lease_fails() => results.FailedLeaseLeftGroupFree.ShouldBeTrue();
    void should_fail_the_scope_whose_release_failed() => results.ScopeEndFailure.ShouldBeSame(results.ReleaseFailure);
    void should_free_the_group_when_a_release_fails() => results.FailedReleaseLeftGroupFree.ShouldBeTrue();
    void should_reject_a_lease_outside_a_specification() => results.LeaseOutsideScopeFailure.ShouldBeOfExactType<InvalidOperationException>();
    void should_reject_a_lease_when_the_project_has_not_opted_in() => results.LeaseWithoutOptInFailure.Message.ShouldContain("CratisSpecificationsSharedResources");
    void should_reject_a_lease_through_the_static_api_when_the_project_has_not_opted_in() => results.LeaseThroughTheStaticApiWithoutOptInFailure.Message.ShouldContain("CratisSpecificationsSharedResources");
    void should_create_nothing_when_rejecting_a_lease_without_opt_in() => results.CreatedWithoutOptIn.ShouldEqual(0);
}
