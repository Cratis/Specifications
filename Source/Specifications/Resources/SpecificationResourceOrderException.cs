// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// The exception that is thrown when a specification leases from limited resource groups out of ordinal key order,
/// which could deadlock against another specification leasing the same groups in the other order.
/// </summary>
/// <param name="heldKey">The key of a group the specification already holds a lease on.</param>
/// <param name="requestedKey">The key of the group the specification asked for.</param>
[Experimental(SpecificationResources.DiagnosticId)]
public sealed class SpecificationResourceOrderException(string heldKey, string requestedKey)
    : Exception(
        $"Leasing from the resource group '{requestedKey}' while holding a lease on '{heldKey}' could deadlock. " +
        "Lease from limited resource groups in ordinal order of their keys, at most once per group.")
{
}
