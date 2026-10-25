// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;

namespace Cratis.Specifications.Runner.given;

internal sealed class a_recording_message_bus : IMessageBus
{
    public List<TestNode> Nodes { get; } = [];

    public Task PublishAsync(IDataProducer producer, IData data)
    {
        if (data is TestNodeUpdateMessage update) Nodes.Add(update.TestNode);
        return Task.CompletedTask;
    }
}
