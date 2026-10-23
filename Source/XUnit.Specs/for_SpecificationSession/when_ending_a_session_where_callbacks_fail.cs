// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationSession;

public class when_ending_a_session_where_callbacks_fail : Specification
{
    SessionRecorder recorder;
    Exception thrown;

    async Task Because()
    {
        recorder = new SessionRecorder()
            .Register("first", new InvalidOperationException("First"))
            .Register("second")
            .Register("third", new InvalidOperationException("Third"));
        try
        {
            await SpecificationSession.End();
        }
        catch (Exception ex)
        {
            thrown = ex;
        }
    }

    [Fact] void should_run_every_callback_the_last_registered_first() => recorder.Ended.ShouldEqual<string>("third", "second", "first");
    [Fact] void should_throw_every_failure_together() => ((AggregateException)thrown).InnerExceptions.Select(_ => _.Message).ShouldEqual<string>("Third", "First");
}
