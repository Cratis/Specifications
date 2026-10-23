// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Turns phase timings on for a test run when they are enabled, and does nothing otherwise.
/// </summary>
/// <remarks>
/// Every adapter package calls <see cref="Initialize"/> from a module initializer. With timings off nothing is
/// registered with the observation seam, so the lifecycle runs exactly as it does without the feature.
/// </remarks>
internal static class PhaseTimings
{
    static int _initialized;
    static PhaseTimingCollector? _collector;

    /// <summary>
    /// Gets the collector, or null when phase timings are off.
    /// </summary>
    public static PhaseTimingCollector? Collector => Volatile.Read(ref _collector);

    /// <summary>
    /// Reads the configuration and, when phase timings are on, starts collecting. Only the first call has an effect.
    /// </summary>
    /// <param name="reportSlow">Reports a line through the native channel of the framework, if it has one.</param>
    public static void Initialize(Action<string>? reportSlow)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1) return;
        Volatile.Write(ref _collector, Start(PhaseTimingOptions.Resolve(), reportSlow));
    }

    /// <summary>
    /// Starts collecting when the options turn phase timings on.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="reportSlow">Reports a line through the native channel of the framework, if it has one.</param>
    /// <param name="observes">Selects the scopes to collect; all of them when null.</param>
    /// <returns>The collector, or null when the options turn phase timings off.</returns>
    public static PhaseTimingCollector? Start(PhaseTimingOptions options, Action<string>? reportSlow, Func<SpecificationScope, bool>? observes = null)
    {
        if (!options.Enabled) return null;

        var collector = new PhaseTimingCollector(options.SlowThreshold, reportSlow, observes);
        SpecificationObservers.Add(collector);
        if (options.FilePath is not null)
        {
            SpecificationSession.Register(() =>
            {
                WriteFile(collector, options.FilePath, reportSlow);
                return default;
            });
        }

        return collector;
    }

    /// <summary>
    /// Gets the phases collected for a scope by the collector observing it.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <returns>The phases, or null when no collector observes the scope.</returns>
    public static IReadOnlyList<PhaseRecord>? PhasesOf(SpecificationScope scope)
    {
        foreach (var observer in SpecificationObservers.All)
        {
            if (observer is PhaseTimingCollector collector && collector.RecordsFor(scope) is { Count: > 0 } phases) return phases;
        }

        return null;
    }

    /// <summary>
    /// Stops a collector from observing further specifications.
    /// </summary>
    /// <param name="collector">The collector.</param>
    public static void Stop(PhaseTimingCollector collector) => SpecificationObservers.Remove(collector);

    /// <summary>
    /// Writes the records of a collector to the timings file. A failure is reported without its message and never thrown.
    /// </summary>
    /// <param name="collector">The collector.</param>
    /// <param name="path">The path of the file.</param>
    /// <param name="report">Reports a line through the native channel of the framework, if it has one.</param>
    public static void WriteFile(PhaseTimingCollector collector, string path, Action<string>? report)
    {
        try
        {
            TimingsJsonWriter.Write(path, collector.Records);
        }
        catch (Exception ex)
        {
            try
            {
                report?.Invoke($"Cratis.Specifications: could not write the phase timings file ({ex.GetType().Name})");
            }
            catch
            {
                // The report is additive.
            }
        }
    }
}
