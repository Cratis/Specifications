// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_PhaseTimings;

public class when_writing_a_slow_phase(ITestOutputHelper output_helper) : Specification
{
    void Because() => PhaseTimingOutput.Write("slow line");

    [Fact] void should_write_the_line_to_the_test_output() => output_helper.Output.ShouldContain("slow line");
}
