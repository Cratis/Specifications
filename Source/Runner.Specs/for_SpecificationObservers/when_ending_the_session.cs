// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;
using Microsoft.Testing.Platform.Extensions.TestFramework;

namespace Cratis.Specifications.Runner.for_SpecificationObservers;

// The only runner specification that ends the session: the callbacks are process wide.
public class when_ending_the_session : Specification
{
    SpecificationFramework framework;
    SessionRecorder recorder;
    CloseTestSessionResult passing;
    CloseTestSessionResult failing;

    void Establish() => framework = new(SpecificationRunOptions.Default, new TrxCapability(), publishInProgress: false);

    async Task Because()
    {
        recorder = new SessionRecorder().Register("first").Register("second");
        passing = await framework.CloseTestSessionAsync(null);

        new SessionRecorder().Register("failing", new InvalidOperationException("Session end failure"));
        failing = await framework.CloseTestSessionAsync(null);
    }

    void should_end_the_session_with_the_last_registered_first() => recorder.Ended.ShouldEqual<string>("second", "first");
    void should_close_the_session_successfully() => passing.IsSuccess.ShouldBeTrue();
    void should_fail_the_session_when_ending_it_fails() => failing.IsSuccess.ShouldBeFalse();
    void should_report_the_failure() => failing.ErrorMessage.ShouldContain("Session end failure");
}
