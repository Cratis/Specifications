// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;
using LifecycleProbes;

namespace StopProbes;

public abstract class running_context : lifecycle_context
{
    protected running_context() => Record("Create");

    async Task Because()
    {
        Record("Started");
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, SpecificationContext.CancellationToken);
        }
        finally
        {
            Record("Cancelled");
        }
    }
}
