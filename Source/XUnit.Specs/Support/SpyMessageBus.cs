// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications.Support;

/// <summary>
/// A message bus that records every message it is handed, for asserting on test execution results.
/// </summary>
public class SpyMessageBus : IMessageBus
{
    /// <summary>
    /// Gets the messages that have been queued.
    /// </summary>
    public List<IMessageSinkMessage> Messages { get; } = [];

    /// <inheritdoc/>
    public bool QueueMessage(IMessageSinkMessage message)
    {
        Messages.Add(message);
        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
