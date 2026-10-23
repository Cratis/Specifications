// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Requests;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the Cratis specification runner as a Microsoft.Testing.Platform test framework.
/// </summary>
/// <param name="options">The run options.</param>
/// <param name="trx">The TRX capability.</param>
/// <param name="publishInProgress">Whether to publish in-progress updates.</param>
internal sealed class SpecificationFramework(SpecificationRunOptions options, TrxCapability trx, bool publishInProgress) : ITestFramework, IDataProducer
{
    /// <inheritdoc/>
    public string Uid => nameof(SpecificationFramework);

    /// <inheritdoc/>
    public string Version => typeof(SpecificationFramework).Assembly.GetName().Version?.ToString() ?? "0.0.0";

    /// <inheritdoc/>
    public string DisplayName => "Cratis Specifications";

    /// <inheritdoc/>
    public string Description => "Runs Cratis specifications: Given/When/Then with one shared context per specification class.";

    /// <inheritdoc/>
    public Type[] DataTypesProduced => [typeof(TestNodeUpdateMessage)];

    /// <inheritdoc/>
    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context) =>
        Task.FromResult(new CreateTestSessionResult { IsSuccess = true });

    /// <inheritdoc/>
    /// <remarks>
    /// Ends the specification session; a failure fails the test session with its message.
    /// </remarks>
    public async Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
    {
        try
        {
            await SpecificationSession.End();
            return new CloseTestSessionResult { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new CloseTestSessionResult { IsSuccess = false, ErrorMessage = $"Ending the specification session failed: {ex}" };
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteRequestAsync(ExecuteRequestContext context)
    {
        try
        {
            if (context.Request is TestExecutionRequest request)
            {
                var nodes = new TestNodeFactory(AssemblyName(), trx);
                var sink = new MessageBusResultSink(this, context.MessageBus, request.Session.SessionUid, nodes, publishInProgress);
                var work = WorkSelector.Select(SpecificationRegistry.Classes, request.Filter);
                if (request is DiscoverTestExecutionRequest)
                {
                    await Discover(work, sink);
                }
                else if (request is RunTestExecutionRequest)
                {
                    await Run(work, sink, context.CancellationToken);
                }
            }
        }
        finally
        {
            context.Complete();
        }
    }

    static async Task Discover(IReadOnlyList<SpecificationWork> work, MessageBusResultSink sink)
    {
        foreach (var item in work)
        {
            foreach (var fact in item.Facts)
            {
                await sink.Discovered(item.Specification, fact);
            }
        }
    }

    static string AssemblyName() => System.Reflection.Assembly.GetEntryAssembly()?.FullName ?? string.Empty;

    async Task Run(IReadOnlyList<SpecificationWork> work, MessageBusResultSink sink, CancellationToken cancellationToken)
    {
        var cache = options.CachePath is null ? RunCache.Empty : RunCache.Load(options.CachePath);
        var plan = WorkPlanner.Plan(work, options, cache);
        var notStarted = await SpecificationScheduler.Run(plan, options, sink, cache, cancellationToken);
        var reason = cancellationToken.IsCancellationRequested ? "The run was cancelled." : "The run stopped after the first failure (--fail-fast).";
        foreach (var item in notStarted)
        {
            foreach (var fact in item.Facts)
            {
                await sink.NotRun(item.Specification, fact, reason);
            }
        }

        if (options.CachePath is not null) cache.Save(options.CachePath);
    }
}
