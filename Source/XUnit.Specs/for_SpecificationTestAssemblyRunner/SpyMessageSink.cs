// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestAssemblyRunner;

public class SpyMessageSink : Xunit.LongLivedMarshalByRefObject, IMessageSink
{
    public List<IMessageSinkMessage> Messages { get; } = [];

    public bool OnMessage(IMessageSinkMessage message)
    {
        lock (Messages) Messages.Add(message);
        return true;
    }
}
