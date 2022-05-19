// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sample.for_AsyncMethods;

public class when_async_given_because_takes_time_to_complete : given.a_slow_because_method
{
    const string EstablishMethod = "establish";

    Task Because()
    {
        methods_called.Add(EstablishMethod);
        return Task.CompletedTask;
    }

    [Fact] void should_finish_the_call_to_given_because_first() => methods_called[0].ShouldEqual(GivenMethod);
}
