// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sample.for_AsyncMethods.given;

public class a_slow_because_method : Specification
{
    protected const string GivenMethod = "given.because";

    protected List<string> methods_called = new();

    async Task Because()
    {
        await Task.Delay(1);
        methods_called.Add(GivenMethod);
    }
}
