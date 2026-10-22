// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Probes;

public class failing_because : a_recording_context
{
    public static readonly InvalidOperationException Failure = new("Because failed");

    void When() => throw Failure;

    void should_fail_first() => CallLog.Record(Probe, "fact");

    void should_fail_second() => CallLog.Record(Probe, "fact");
}

public class establish_and_given : Specification
{
    void Establish() => CallLog.Record(nameof(establish_and_given), "Establish");

    void Given() => CallLog.Record(nameof(establish_and_given), "Given");

    void should_not_run() => CallLog.Record(nameof(establish_and_given), "fact");
}

public class failing_fact_and_skipped_fact : Specification
{
    void should_pass() { }

    void should_fail() => throw new InvalidOperationException("Fact failed");

    [Fact(Skip = "Not yet")] void should_be_skipped() => throw new InvalidOperationException("Skipped fact ran");
}

public class failing_destroy : Specification
{
    void Destroy() => throw new InvalidOperationException("Destroy failed");

    void should_pass_until_destroy_fails() { }
}

public class slow_specification : Specification
{
    async Task Because() => await Task.Delay(50);

    void should_take_a_while() { }
}

public class without_facts : Specification
{
    void Establish() => CallLog.Record(nameof(without_facts), "Establish");
}
