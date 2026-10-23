// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers;

// Exercises the set-up fixture the package compiles into the test project, the one NUnit tears down at the end.
[NonParallelizable]
public class when_ending_the_session : Specification
{
    static readonly InvalidOperationException failure = new("Session end failure");

    SessionRecorder recorder;
    Exception thrown;

    async Task Because()
    {
        recorder = new SessionRecorder().Register("first").Register("second");
        await new CratisSpecificationsSession().End();

        new SessionRecorder().Register("failing", failure);
        try
        {
            await new CratisSpecificationsSession().End();
        }
        catch (Exception ex)
        {
            thrown = ex;
        }
    }

    [Test] public void should_end_the_session_with_the_last_registered_first() => recorder.Ended.ShouldEqual<string>("second", "first");
    [Test] public void should_surface_a_failure_to_nunit() => thrown.ShouldEqual(failure);
}
