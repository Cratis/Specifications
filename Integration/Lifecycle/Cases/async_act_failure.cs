// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LifecycleProbes;

#if TUNIT
[global::TUnit.Core.InheritsTests]
#endif
public class async_act_failure : lifecycle_context
{
    async ValueTask Because()
    {
        await Task.Yield();
        throw new InvalidOperationException("deliberate act failure");
    }
}
