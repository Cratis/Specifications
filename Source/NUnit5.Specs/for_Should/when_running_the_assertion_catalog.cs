// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Assertions;

namespace Cratis.Specifications.for_Should;

public class when_running_the_assertion_catalog : Specification
{
    IReadOnlyList<string> _mismatches;
    IReadOnlyList<string> _message_mismatches;
    Exception _plain_class_failure;

    void Because()
    {
        _mismatches = AssertionCatalog.Verify(typeof(AssertionException), action => Assert.Catch(() => action()));
        _message_mismatches = AssertionMessages.Verify(action => Assert.Catch(() => action()));
        _plain_class_failure = Catch.Exception(new PlainAsserter().AssertSomethingFalse);
    }

    [Test] public void should_behave_as_catalogued_for_every_case() => string.Join(Environment.NewLine, _mismatches).ShouldEqual(string.Empty);
    [Test] public void should_have_catalogued_cases() => AssertionCases.All.ShouldNotBeEmpty();
    [Test] public void should_fail_with_the_same_messages_as_every_package() => string.Join(Environment.NewLine, _message_mismatches).ShouldEqual(string.Empty);
    [Test] public void should_fail_from_a_plain_class_with_the_package_exception() => _plain_class_failure.ShouldBeOfExactType<AssertionException>();
}
