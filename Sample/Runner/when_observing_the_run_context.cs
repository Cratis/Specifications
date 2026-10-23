// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sample;

public class when_observing_the_run_context : Specification
{
    CancellationToken token;

    // Async work can pass this token to cancellable APIs. Destroy and disposal still run after cancellation.
    async Task Given()
    {
        token = SpecificationContext.CancellationToken;
        await Task.Yield();
    }

    void should_keep_the_same_token_across_awaits() => SpecificationContext.CancellationToken.ShouldEqual(token);
}
