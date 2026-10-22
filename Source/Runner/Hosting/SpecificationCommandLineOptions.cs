// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Provides the command line options of the Cratis specification runner.
/// </summary>
internal sealed class SpecificationCommandLineOptions : ICommandLineOptionsProvider
{
    /// <summary>The maximum number of specification classes running at the same time.</summary>
    public const string Parallel = "specs-parallel";

    /// <summary>Runs the facts of a class one by one.</summary>
    public const string SequentialFacts = "specs-sequential-facts";

    /// <summary>Stops starting new classes after the first failure.</summary>
    public const string FailFast = "fail-fast";

    /// <summary>Starts the classes that failed in the previous run first.</summary>
    public const string LastFailedFirst = "last-failed-first";

    /// <summary>Runs one shard of the classes.</summary>
    public const string Shard = "shard";

    /// <summary>The path of the run cache file, or "none".</summary>
    public const string Cache = "specs-cache";

    /// <summary>The environment variable giving the maximum number of specification classes running at the same time.</summary>
    public const string ParallelEnvironmentVariable = "CRATIS_SPECS_PARALLEL";

    /// <inheritdoc/>
    public string Uid => nameof(SpecificationCommandLineOptions);

    /// <inheritdoc/>
    public string Version => "1.0.0";

    /// <inheritdoc/>
    public string DisplayName => "Cratis Specifications";

    /// <inheritdoc/>
    public string Description => "Options of the Cratis specification runner.";

    /// <inheritdoc/>
    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    /// <inheritdoc/>
    public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() =>
    [
        new(Parallel, $"The maximum number of specification classes running at the same time. Defaults to {ParallelEnvironmentVariable} or the processor count; 1 runs classes one by one.", ArgumentArity.ExactlyOne, false),
        new(SequentialFacts, "Runs the facts of a specification class one by one instead of concurrently.", ArgumentArity.Zero, false),
        new(FailFast, "Stops starting new specification classes after the first failure.", ArgumentArity.Zero, false),
        new(LastFailedFirst, "Starts the specification classes that failed in the previous run first.", ArgumentArity.Zero, false),
        new(Shard, "Runs one shard of the specification classes, as index/count (for instance 2/4). Balanced by the durations in the run cache when present, otherwise split by a stable hash.", ArgumentArity.ExactlyOne, false),
        new(Cache, "The path of the run cache remembering durations and failures, or 'none'. Defaults to .specs-cache next to the test assembly.", ArgumentArity.ExactlyOne, false)
    ];

    /// <inheritdoc/>
    public Task<ValidationResult> ValidateOptionArgumentsAsync(CommandLineOption commandOption, string[] arguments) => commandOption.Name switch
    {
        Parallel when !IsPositiveInteger(arguments[0]) => ValidationResult.InvalidTask($"--{Parallel} must be a positive integer."),
        Shard when !Runner.Shard.TryParse(arguments[0], out _) => ValidationResult.InvalidTask($"--{Shard} must be index/count, for instance 2/4."),
        _ => ValidationResult.ValidTask
    };

    /// <inheritdoc/>
    public Task<ValidationResult> ValidateCommandLineOptionsAsync(ICommandLineOptions commandLineOptions) => ValidationResult.ValidTask;

    /// <summary>
    /// Reads the run options.
    /// </summary>
    /// <param name="commandLine">The parsed command line.</param>
    /// <returns>The run options.</returns>
    public static SpecificationRunOptions Read(ICommandLineOptions commandLine)
    {
        var parallel = Environment.ProcessorCount;
        if (IsPositiveInteger(Environment.GetEnvironmentVariable(ParallelEnvironmentVariable))) parallel = int.Parse(Environment.GetEnvironmentVariable(ParallelEnvironmentVariable)!, CultureInfo.InvariantCulture);
        if (commandLine.TryGetOptionArgumentList(Parallel, out var parallelArguments)) parallel = int.Parse(parallelArguments[0], CultureInfo.InvariantCulture);

        Shard? shard = commandLine.TryGetOptionArgumentList(Shard, out var shardArguments) && Runner.Shard.TryParse(shardArguments[0], out var parsed) ? parsed : null;

        var cache = Path.Combine(AppContext.BaseDirectory, ".specs-cache");
        if (commandLine.TryGetOptionArgumentList(Cache, out var cacheArguments)) cache = cacheArguments[0];

        return new(
            parallel,
            commandLine.IsOptionSet(SequentialFacts),
            commandLine.IsOptionSet(FailFast),
            commandLine.IsOptionSet(LastFailedFirst),
            shard,
            string.Equals(cache, "none", StringComparison.OrdinalIgnoreCase) ? null : cache);
    }

    static bool IsPositiveInteger(string? value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0;
}
