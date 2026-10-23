// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the work of running a selection of facts of one specification class.
/// </summary>
/// <param name="Specification">The specification class.</param>
/// <param name="Facts">The selected facts.</param>
internal sealed record SpecificationWork(SpecificationClassDescriptor Specification, IReadOnlyList<FactDescriptor> Facts);
