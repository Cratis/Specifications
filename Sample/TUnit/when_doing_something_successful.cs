// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sample;

public class when_doing_something_successful
{
    void Establish() { }

    void Because() { }

    [Test] public void should_be_true() => true.ShouldBeTrue();
}
