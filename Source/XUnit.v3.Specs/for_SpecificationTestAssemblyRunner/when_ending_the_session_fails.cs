// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Cratis.Specifications.for_SpecificationTestAssemblyRunner;

// Runs an assembly with no test cases: the runner still ends the session once its collections are done.
public class when_ending_the_session_fails : Specification
{
    readonly List<IMessageSinkMessage> messages = [];
    SessionRecorder recorder;

    async Task Because()
    {
        recorder = new SessionRecorder().Register("failing", new InvalidOperationException("Session end failure"));
        await SpecificationTestAssemblyRunner.Instance.Run(
            new XunitTestAssembly(typeof(when_ending_the_session_fails).Assembly, configFilePath: null),
            [],
            new SpyMessageSink(messages),
            TestFrameworkOptions.ForExecution(new TestAssemblyConfiguration()),
            CancellationToken.None);
    }

    [Fact] void should_end_the_session() => recorder.Ended.ShouldEqual<string>("failing");
    [Fact] void should_report_an_assembly_cleanup_failure() => messages.OfType<ITestAssemblyCleanupFailure>().Single().Messages.ShouldContain("Session end failure");
    [Fact] void should_still_finish_the_assembly() => messages.OfType<ITestAssemblyFinished>().Count().ShouldEqual(1);

    sealed class SpyMessageSink(List<IMessageSinkMessage> messages) : IMessageSink
    {
        public bool OnMessage(IMessageSinkMessage message)
        {
            lock (messages) messages.Add(message);
            return true;
        }
    }
}
