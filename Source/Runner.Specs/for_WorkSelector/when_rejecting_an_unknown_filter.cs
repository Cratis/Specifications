// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Requests;

namespace Cratis.Specifications.Runner.for_WorkSelector;

public class when_rejecting_an_unknown_filter : Specification
{
    NotSupportedException exception;

    void Because()
    {
        try
        {
            WorkSelector.Select(given.the_probes.All, new unknown_filter());
        }
        catch (NotSupportedException error)
        {
            exception = error;
        }
    }

    void should_fail_explicitly() => exception.ShouldNotBeNull();

    void should_preserve_the_supported_filter_hint() => exception.Message.ShouldEqual("The filter 'unknown_filter' is not supported by the Cratis specification runner; use --filter-uid.");

    sealed class unknown_filter : ITestExecutionFilter;
}
