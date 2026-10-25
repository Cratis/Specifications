// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Keeps completed fact outcomes separate from a failure cleaning up their shared context.
/// </summary>
/// <param name="Facts">The results of the selected facts, in order.</param>
/// <param name="TearDownFailure">The separate teardown result, if cleanup failed.</param>
internal sealed record SpecificationClassResult(FactResult[] Facts, FactResult? TearDownFailure = null);
