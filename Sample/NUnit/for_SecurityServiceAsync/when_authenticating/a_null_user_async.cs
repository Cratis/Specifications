// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sample.for_SecurityServiceAsync;

public class When_authenticating_a_null_user_async : given.no_user_authenticated
{
    Exception result;

    Task Establish()
    {
        Console.WriteLine("Establish in specification");
        return Task.Delay(50);
    }

    async Task Because() => result = await Catch.Exception(() => subject.AuthenticateAsync(null, null));

    [Test] public void should_throw_user_must_be_specified_exception() => result.ShouldBeOfExactType<UserMustBeSpecified>();
}
