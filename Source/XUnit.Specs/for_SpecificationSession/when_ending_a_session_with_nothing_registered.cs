// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationSession;

public class when_ending_a_session_with_nothing_registered : Specification
{
    Task end;

    void Because() => end = SpecificationSessionHook.End();

    [Fact] void should_complete_without_waiting() => end.ShouldEqual(Task.CompletedTask);
}
