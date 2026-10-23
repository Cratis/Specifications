// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// Describes how a probe resource behaves when started, leased and stopped.
/// </summary>
internal sealed class ProbeBehavior
{
    public Task StartGate { get; init; } = Task.CompletedTask;
    public Exception StartFailure { get; init; }
    public Exception StopFailure { get; init; }
    public bool StopHangs { get; init; }
    public Exception LeaseFailure { get; init; }
    public Exception ReleaseFailure { get; init; }
}
