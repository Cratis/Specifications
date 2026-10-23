// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// Starts a resource that fails to stop, on a registry that stops with the real test session.
/// </summary>
/// <remarks>
/// Each package ends the session through its own framework channel and asserts the stop failure reaches it.
/// </remarks>
internal static class ResourceSessionCase
{
    public const string StopFailureMessage = "Shared resource stop failure";

    public static async Task<ResourceLog> StartResourceThatFailsToStop()
    {
        var log = new ResourceLog();
        var registry = new ResourceRegistry();
        await registry.Get(() => new unlimited_resource(log, new() { StopFailure = new InvalidOperationException(StopFailureMessage) }), default);
        return log;
    }
}
