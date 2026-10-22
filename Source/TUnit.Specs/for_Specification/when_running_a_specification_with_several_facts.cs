// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.probes;

namespace Cratis.Specifications.for_Specification;

/// <summary>
/// Runs for real on TUnit: if the context was not shared, Establish would run once per fact and the later facts would fail.
/// </summary>
public class when_running_a_specification_with_several_facts : Specification
{
    static int _establishCalls;
    static int _becauseCalls;
    static int _becauseSawEstablish;

    void Establish() => Interlocked.Increment(ref _establishCalls);

    void Because()
    {
        Interlocked.Increment(ref _becauseCalls);
        _becauseSawEstablish = _establishCalls;
    }

    void Destroy() => destroy_tracker.Destroyed();

    [Test] public void should_establish_once_for_the_first_fact() => _establishCalls.ShouldEqual(1);
    [Test] public void should_establish_once_for_the_second_fact() => _establishCalls.ShouldEqual(1);
    [Test] public void should_establish_once_for_the_third_fact() => _establishCalls.ShouldEqual(1);
    [Test] public void should_perform_the_action_once() => _becauseCalls.ShouldEqual(1);
    [Test] public void should_perform_the_action_after_establishing() => _becauseSawEstablish.ShouldEqual(1);
}
