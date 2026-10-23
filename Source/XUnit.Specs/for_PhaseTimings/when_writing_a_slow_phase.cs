// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications.for_PhaseTimings;

public class when_writing_a_slow_phase : Specification
{
    sealed class RecordingSink : LongLivedMarshalByRefObject, IMessageSink
    {
        public List<IMessageSinkMessage> Messages { get; } = [];

        public bool OnMessage(IMessageSinkMessage message)
        {
            Messages.Add(message);
            return true;
        }
    }

    RecordingSink sink;

    void Because()
    {
        sink = new RecordingSink();
        PhaseTimingOutput.UseDiagnosticSink(sink);
        PhaseTimingOutput.Write("slow line");
    }

    [Fact] void should_send_a_single_message() => sink.Messages.Count.ShouldEqual(1);
    [Fact] void should_send_it_as_a_diagnostic_message() => sink.Messages[0].ShouldBeOfExactType<DiagnosticMessage>();
    [Fact] void should_carry_the_line() => ((DiagnosticMessage)sink.Messages[0]).Message.ShouldEqual("slow line");
}
