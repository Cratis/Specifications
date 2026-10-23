// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Describes the specification a lease on a shared resource is for.
/// </summary>
/// <param name="Specification">The type of the specification.</param>
/// <param name="DisplayName">The display name of the specification.</param>
/// <param name="LeaseId">An identifier unique to the lease within the test session, usable to name isolated parts such as a database.</param>
[Experimental(SpecificationResources.DiagnosticId)]
public sealed record SpecificationLeaseContext(Type Specification, string DisplayName, string LeaseId);
