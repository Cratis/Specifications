// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Runner.for_PhaseTimings.given;

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_a_phase_fails : Specification
{
    const string Uid = "Cratis.Specifications.Runner.for_PhaseTimings.when_a_phase_fails";

    readonly List<string> slow = [];
    IReadOnlyList<PhaseRecord> records;
    string json;

    IEnumerable<string> Slow()
    {
        lock (slow) return [.. slow];
    }

    async Task Because()
    {
        var collector = PhaseTimings.Start(new PhaseTimingOptions(true, TimeSpan.Zero, null), line => { lock (slow) slow.Add(line); }, scope => scope.DisplayName == Uid);
        try
        {
            await SpecificationClassRunner.Run(timed_work.For(Uid, failBecause: true), sequentialFacts: false);
            records = [.. collector.Records.Where(_ => _.Specification == Uid)];
            using var stream = new MemoryStream();
            TimingsJsonWriter.Write(stream, records);
            json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        finally
        {
            PhaseTimings.Stop(collector);
        }
    }

    void should_mark_the_failed_phase() => records.Single(_ => _.Phase == SpecificationPhase.Because).Failed.ShouldBeTrue();
    void should_not_put_the_exception_text_in_the_report() => Slow().Where(_ => _.Contains(Uid)).ShouldEachConformTo(_ => !_.Contains("Observed"));
    void should_not_put_the_exception_text_in_the_json() => json.ShouldNotContain("Observed");
}
