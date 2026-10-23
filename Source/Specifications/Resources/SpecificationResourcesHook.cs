// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Cratis.Specifications.Resources;

/// <summary>
/// The entry point the module initializer compiled into a test project that sets the MSBuild property
/// <c>CratisSpecificationsSharedResources</c> to <c>true</c> calls. Not intended to be used directly.
/// </summary>
/// <remarks>
/// Enabling registers the observer that releases leases after Destroy. Until then nothing observes the lifecycle on
/// behalf of shared resources, so a test project that does not use them keeps the unobserved fast path.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class SpecificationResourcesHook
{
    /// <summary>
    /// The MSBuild property a test project sets to <c>true</c> to use leases on shared resources.
    /// </summary>
    public const string OptInProperty = "CratisSpecificationsSharedResources";

    static int _enabled;

    /// <summary>
    /// Gets a value indicating whether the test project opted in to shared resource leases.
    /// </summary>
    public static bool IsEnabled => Volatile.Read(ref _enabled) == 1;

    /// <summary>
    /// Enables leases on shared resources for the test session. Calling it again does nothing.
    /// </summary>
    public static void Enable()
    {
        if (Interlocked.Exchange(ref _enabled, 1) == 1) return;
#pragma warning disable CRSPECEXP01 // The hook is the stable entry point into the experimental engine.
        SpecificationObservers.Add(new ResourceLeaseObserver());
#pragma warning restore CRSPECEXP01
    }
}
