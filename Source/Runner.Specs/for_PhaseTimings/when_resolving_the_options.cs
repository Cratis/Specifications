// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_resolving_the_options : Specification
{
    PhaseTimingOptions defaults;
    PhaseTimingOptions from_environment;
    PhaseTimingOptions from_metadata;
    PhaseTimingOptions with_bad_threshold;

    void Because()
    {
        defaults = PhaseTimingOptions.Resolve(new(false, null), new(null, null, null));
        from_environment = PhaseTimingOptions.Resolve(new(true, "environment.json"), new(null, "metadata.json", "250"));
        from_metadata = PhaseTimingOptions.Resolve(new(false, null), new("true", "metadata.json", "250"));
        with_bad_threshold = PhaseTimingOptions.Resolve(new(false, null), new("true", null, "slow"));
    }

    void should_be_off_by_default() => defaults.Enabled.ShouldBeFalse();
    void should_default_the_threshold_to_one_second() => defaults.SlowThreshold.ShouldEqual(TimeSpan.FromMilliseconds(1000));
    void should_have_no_file_by_default() => defaults.FilePath.ShouldBeNull();
    void should_be_turned_on_by_the_environment() => from_environment.Enabled.ShouldBeTrue();
    void should_prefer_the_file_from_the_environment() => from_environment.FilePath.ShouldEqual("environment.json");
    void should_be_turned_on_by_the_metadata() => from_metadata.Enabled.ShouldBeTrue();
    void should_use_the_file_from_the_metadata() => from_metadata.FilePath.ShouldEqual("metadata.json");
    void should_read_the_threshold_in_milliseconds() => from_metadata.SlowThreshold.ShouldEqual(TimeSpan.FromMilliseconds(250));
    void should_fall_back_to_the_default_threshold_when_it_is_not_a_number() => with_bad_threshold.SlowThreshold.ShouldEqual(PhaseTimingOptions.DefaultSlowThreshold);
}
