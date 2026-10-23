// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;
using Cratis.Specifications.Support;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications.for_SpecificationTestAssemblyRunner;

// Runs an assembly with no test cases: the runner still ends the session once its collections are done.
public class when_ending_the_session_fails : Specification
{
    SpyMessageSink sink;
    SessionRecorder recorder;

    async Task Because()
    {
        sink = new SpyMessageSink();
        recorder = new SessionRecorder().Register("failing", new InvalidOperationException("Session end failure"));
        using var runner = new SpecificationTestAssemblyRunner(
            new TestAssembly(Reflector.Wrap(typeof(when_ending_the_session_fails).Assembly)),
            [],
            new NullDiagnosticMessageSink(),
            sink,
            new DefaultExecutionOptions());
        await runner.RunAsync();
    }

    [Fact] void should_end_the_session() => recorder.Ended.ShouldEqual<string>("failing");
    [Fact] void should_report_an_assembly_cleanup_failure() => sink.Messages.OfType<ITestAssemblyCleanupFailure>().Single().Messages.ShouldContain("Session end failure");
    [Fact] void should_still_finish_the_assembly() => sink.Messages.OfType<ITestAssemblyFinished>().Count().ShouldEqual(1);
}
