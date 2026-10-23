// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Extensions.TrxReport.Abstractions;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the capability of reporting TRX specific properties, enabled by the TRX report extension when it is used.
/// </summary>
internal sealed class TrxCapability : ITrxReportCapability
{
    /// <summary>
    /// Gets a value indicating whether TRX reporting is enabled for this run.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <inheritdoc/>
    public bool IsSupported => true;

    /// <inheritdoc/>
    public void Enable() => IsEnabled = true;
}
