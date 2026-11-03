// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

// The condition lives on the base class, so the runner only finds it when its lookup includes inherited static members.
abstract class a_specification_with_a_skip_condition : Specification
{
    public static bool ConditionHolds => true;
}
