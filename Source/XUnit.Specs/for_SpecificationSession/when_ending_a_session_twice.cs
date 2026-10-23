// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationSession;

public class when_ending_a_session_twice : Specification
{
    SessionRecorder recorder;

    async Task Because()
    {
        recorder = new SessionRecorder().Register("only");
        await SpecificationSession.End();
        await SpecificationSession.End();
    }

    [Fact] void should_run_the_callbacks_once() => recorder.Ended.ShouldEqual<string>("only");
}
