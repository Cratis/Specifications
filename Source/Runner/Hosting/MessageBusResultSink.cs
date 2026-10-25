// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.TestHost;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Publishes specification results to the Microsoft.Testing.Platform message bus.
/// </summary>
/// <remarks>
/// Results are published once per class, after the class has completed. A failing Destroy or disposal is
/// reported separately from its facts. In-progress updates are only published when a client shows them (server mode, used by IDEs);
/// on the command line they would double the number of messages for nothing.
/// </remarks>
/// <param name="producer">The data producer publishing the messages.</param>
/// <param name="bus">The message bus.</param>
/// <param name="session">The test session.</param>
/// <param name="nodes">The factory creating the test nodes.</param>
/// <param name="publishInProgress">Whether to publish in-progress updates.</param>
internal sealed class MessageBusResultSink(IDataProducer producer, IMessageBus bus, SessionUid session, TestNodeFactory nodes, bool publishInProgress) : ISpecificationResultSink
{
    /// <inheritdoc/>
    public async ValueTask Starting(SpecificationWork work)
    {
        if (!publishInProgress) return;
        foreach (var fact in work.Facts)
        {
            await Publish(nodes.Create(work.Specification, fact, InProgressTestNodeStateProperty.CachedInstance));
        }
    }

    /// <inheritdoc/>
    public async ValueTask Completed(SpecificationWork work, IReadOnlyList<FactResult> results, FactResult? teardownFailure = null)
    {
        for (var index = 0; index < results.Count; index++)
        {
            await Publish(nodes.Create(work.Specification, work.Facts[index], results[index]));
        }

        if (teardownFailure is { } failure) await Publish(nodes.CreateTearDown(work.Specification, failure));
    }

    /// <summary>
    /// Publishes a fact as discovered.
    /// </summary>
    /// <param name="specification">The specification class.</param>
    /// <param name="fact">The fact.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Discovered(SpecificationClassDescriptor specification, FactDescriptor fact) =>
        Publish(nodes.Create(specification, fact, DiscoveredTestNodeStateProperty.CachedInstance));

    /// <summary>
    /// Publishes a fact as cancelled because the run stopped before its class started.
    /// </summary>
    /// <param name="specification">The specification class.</param>
    /// <param name="fact">The fact.</param>
    /// <param name="reason">Why it did not run.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task NotRun(SpecificationClassDescriptor specification, FactDescriptor fact, string reason) =>
        Publish(nodes.Create(specification, fact, new SkippedTestNodeStateProperty(reason)));

    Task Publish(TestNode node) => bus.PublishAsync(producer, new TestNodeUpdateMessage(session, node));
}
