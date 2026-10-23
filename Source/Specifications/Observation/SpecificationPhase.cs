// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Defines the phases of a specification that are reported to an <see cref="ISpecificationObserver"/>.
/// </summary>
/// <remarks>
/// The adapters report <see cref="Establish"/>, <see cref="Because"/> and <see cref="Destroy"/> for every observed
/// scope. The other phases are reserved for the features that plug into the seam (shared resources, phase timings).
/// </remarks>
internal enum SpecificationPhase
{
    /// <summary>Waiting to acquire a shared resource.</summary>
    AcquireWait = 0,

    /// <summary>Starting a shared resource.</summary>
    ResourceStart = 1,

    /// <summary>Leasing a shared resource to a scope.</summary>
    Lease = 2,

    /// <summary>The Establish (Given) lifecycle methods.</summary>
    Establish = 3,

    /// <summary>The Because (When) lifecycle methods.</summary>
    Because = 4,

    /// <summary>A fact.</summary>
    Fact = 5,

    /// <summary>The Destroy lifecycle methods.</summary>
    Destroy = 6,

    /// <summary>Releasing a lease on a shared resource.</summary>
    LeaseRelease = 7,

    /// <summary>Stopping a shared resource.</summary>
    ResourceStop = 8,

    /// <summary>The end of the test session.</summary>
    SessionEnd = 9
}
