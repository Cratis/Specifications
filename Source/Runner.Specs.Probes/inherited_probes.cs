// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Probes;

public abstract class a_context_with_a_fact : Specification
{
    void should_be_declared_on_the_base() { }
}

public class inheriting_a_fact : a_context_with_a_fact
{
    void should_be_declared_on_the_derived_type() { }
}
