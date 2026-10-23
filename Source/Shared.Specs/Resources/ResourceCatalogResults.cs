// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// What the shared resources did in each case of the <see cref="ResourceCatalog"/>.
/// </summary>
internal sealed class ResourceCatalogResults
{
    public static readonly string[] ReleaseOrder = ["lease a", "lease b", "lease unlimited", "established", "release unlimited", "release b", "release a"];

    public int StartsForParallelAcquirers { get; set; }
    public int CreatedForParallelAcquirers { get; set; }
    public int InstancesForParallelAcquirers { get; set; }
    public int SessionEndRegistrations { get; set; }

    public int CreatedWhenNeverAcquired { get; set; }
    public int SessionEndRegistrationsWhenNeverAcquired { get; set; }

    public Exception StartFailure { get; set; }
    public Exception FirstAcquirerFailure { get; set; }
    public Exception SecondAcquirerFailure { get; set; }
    public int StartsAfterFailure { get; set; }
    public int DisposalsAfterFailedStart { get; set; }
    public Exception PartialStartCleanupFailure { get; set; }
    public Exception ReportedPartialStartCleanupFailure { get; set; }

    public IReadOnlyList<string> ReleaseSequence { get; set; }
    public string LeaseDisplayName { get; set; }
    public bool LeaseIdsAreUnique { get; set; }

    public int PeakForExclusiveGroup { get; set; }
    public bool IndependentGroupsLeasedTogether { get; set; }
    public int PeakForIndependentGroups { get; set; }

    public Exception OutOfOrderFailure { get; set; }
    public bool OutOfOrderLeaseLeftGroupFree { get; set; }

    public Exception CancelledWaitingForLease { get; set; }
    public Exception CancelledWaitingForStart { get; set; }
    public bool StartCarriedOnAfterCancelledWait { get; set; }

    public Exception StopFailure { get; set; }
    public Exception ReportedStopFailures { get; set; }
    public IReadOnlyList<string> StopSequence { get; set; }

    public bool BorrowedResourceReturned { get; set; }
    public int BorrowedStarts { get; set; }
    public int BorrowedDisposals { get; set; }

    public Exception LeaseFailure { get; set; }
    public Exception FailedLeaseEstablishFailure { get; set; }
    public bool FailedLeaseLeftGroupFree { get; set; }

    public Exception ReleaseFailure { get; set; }
    public Exception ScopeEndFailure { get; set; }
    public bool FailedReleaseLeftGroupFree { get; set; }

    public Exception LeaseOutsideScopeFailure { get; set; }

    public Exception LeaseWithoutOptInFailure { get; set; }
    public Exception LeaseThroughTheStaticApiWithoutOptInFailure { get; set; }
    public int CreatedWithoutOptIn { get; set; }
}
