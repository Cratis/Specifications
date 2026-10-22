// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents a request to stop scheduling further specification classes, for instance after the first failure.
/// </summary>
internal sealed class ExecutionControl
{
    volatile bool _stopRequested;

    /// <summary>
    /// Gets a value indicating whether a stop was requested.
    /// </summary>
    public bool StopRequested => _stopRequested;

    /// <summary>
    /// Requests that no further specification classes are started. Running classes complete.
    /// </summary>
    public void RequestStop() => _stopRequested = true;
}
