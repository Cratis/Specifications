// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.given;

public class a_specification_class_constructor : Specification
{
    protected SpecificationClassConstructor constructor;

    void Establish() => constructor = new();

    protected async Task<T> Create<T>()
        where T : Specification => (T)await constructor.Create(typeof(T), null);
}
