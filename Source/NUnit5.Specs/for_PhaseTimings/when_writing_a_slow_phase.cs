// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_PhaseTimings;

public class when_writing_a_slow_phase : Specification
{
    StringWriter writer;

    void Because()
    {
        writer = new StringWriter();
        PhaseTimingOutput.Write(writer, "slow line");
        PhaseTimingOutput.Write("slow line");
    }

    [Test] public void should_write_the_line() => writer.ToString().ShouldContain("slow line");
}
