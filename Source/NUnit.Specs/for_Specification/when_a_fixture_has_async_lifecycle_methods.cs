// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

public class when_a_fixture_has_async_lifecycle_methods : Specification
{
    bool _established;
    bool _became;
    bool _established_before_because;

    async Task Establish()
    {
        await Task.Delay(20);
        _established = true;
    }

    async ValueTask Because()
    {
        _established_before_because = _established;
        await Task.Delay(20);
        _became = true;
    }

    [Test] public void should_await_the_task_returning_establish() => _established_before_because.ShouldBeTrue();
    [Test] public void should_await_the_value_task_returning_because() => _became.ShouldBeTrue();
}
