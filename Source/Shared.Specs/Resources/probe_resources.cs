// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>A probe resource without limits.</summary>
internal sealed class unlimited_resource(ResourceLog log, ProbeBehavior behavior = null) : ProbeResource("unlimited", log, behavior);

/// <summary>A probe resource with exclusive leases in the group "a".</summary>
[SpecificationResource(Key = "a", MaxConcurrency = 1)]
internal sealed class first_exclusive_resource(ResourceLog log, ProbeBehavior behavior = null) : ProbeResource("a", log, behavior);

/// <summary>A probe resource with exclusive leases in the group "b".</summary>
[SpecificationResource(Key = "b", MaxConcurrency = 1)]
internal sealed class second_exclusive_resource(ResourceLog log, ProbeBehavior behavior = null) : ProbeResource("b", log, behavior);

/// <summary>A probe resource that may take one second to stop.</summary>
[SpecificationResource(StopTimeoutSeconds = 1)]
internal sealed class quickly_stopped_resource(ResourceLog log, ProbeBehavior behavior = null) : ProbeResource("quick", log, behavior);
