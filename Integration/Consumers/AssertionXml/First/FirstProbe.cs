// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;

namespace Consumer.AssertionXml;

public static class FirstProbe
{
    public static void Fail() => 1.ShouldEqual(2);
}
