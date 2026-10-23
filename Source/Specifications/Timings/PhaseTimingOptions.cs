// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Reflection;

namespace Cratis.Specifications;

/// <summary>
/// The configuration of phase timings, read from the environment and the AssemblyMetadata of the test assemblies.
/// </summary>
/// <param name="Enabled">Whether phase timings are on.</param>
/// <param name="SlowThreshold">The duration above which a phase is reported as slow.</param>
/// <param name="FilePath">The path of the JSON timings file, if any.</param>
internal sealed record PhaseTimingOptions(bool Enabled, TimeSpan SlowThreshold, string? FilePath)
{
    /// <summary>The environment variable that turns timings on.</summary>
    public const string EnabledVariable = "CRATIS_SPECS_TIMINGS";

    /// <summary>The environment variable naming the JSON timings file.</summary>
    public const string FileVariable = "CRATIS_SPECS_TIMINGS_FILE";

    /// <summary>The AssemblyMetadata key that turns timings on.</summary>
    public const string EnabledKey = "CratisSpecificationsPhaseTimings";

    /// <summary>The AssemblyMetadata key naming the JSON timings file.</summary>
    public const string FileKey = "CratisSpecificationsPhaseTimingsFile";

    /// <summary>The AssemblyMetadata key holding the slow phase threshold in milliseconds.</summary>
    public const string ThresholdKey = "CratisSpecificationsSlowPhaseThreshold";

    /// <summary>
    /// The default slow phase threshold.
    /// </summary>
    public static readonly TimeSpan DefaultSlowThreshold = TimeSpan.FromMilliseconds(1000);

    static readonly string[] _frameworkPrefixes = ["System.", "Microsoft.", "xunit", "nunit", "TUnit", "testhost"];
    static readonly string[] _frameworkNames = ["System", "mscorlib", "netstandard", "Cratis.Specifications", "Cratis.Specifications.XUnit", "Cratis.Specifications.XUnit.v3", "Cratis.Specifications.NUnit", "Cratis.Specifications.NUnit5", "Cratis.Specifications.TUnit", "Cratis.Specifications.Runner"];

    /// <summary>
    /// Resolves the options from the environment and the loaded test assemblies.
    /// </summary>
    /// <returns>The options.</returns>
    public static PhaseTimingOptions Resolve()
    {
        var environment = IsTrue(Environment.GetEnvironmentVariable(EnabledVariable));
        var file = Environment.GetEnvironmentVariable(FileVariable);

        // Without both environment variables the AssemblyMetadata of the loaded assemblies decides; this is read once at startup.
        var metadata = environment && !string.IsNullOrWhiteSpace(file) ? default : AssemblyMetadataValues.Read();
        return Resolve(new(environment, file), metadata);
    }

    /// <summary>
    /// Resolves the options from already read values.
    /// </summary>
    /// <param name="environment">The values read from the environment.</param>
    /// <param name="metadata">The values read from the AssemblyMetadata of the test assemblies.</param>
    /// <returns>The options.</returns>
    public static PhaseTimingOptions Resolve(EnvironmentValues environment, AssemblyMetadataValues metadata)
    {
        var enabled = environment.Enabled || IsTrue(metadata.Enabled);
        var file = string.IsNullOrWhiteSpace(environment.File) ? metadata.File : environment.File;
        var threshold = double.TryParse(metadata.Threshold, NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds) && milliseconds >= 0
            ? TimeSpan.FromMilliseconds(milliseconds)
            : DefaultSlowThreshold;
        return new(enabled, threshold, string.IsNullOrWhiteSpace(file) ? null : file);
    }

    /// <summary>
    /// The phase timing values read from the environment.
    /// </summary>
    /// <param name="Enabled">Whether the environment variable turned timings on.</param>
    /// <param name="File">The timings file, if any.</param>
    internal readonly record struct EnvironmentValues(bool Enabled, string? File);

    /// <summary>
    /// The phase timing values read from the AssemblyMetadata of the test assemblies.
    /// </summary>
    /// <param name="Enabled">The enabled value, if any.</param>
    /// <param name="File">The timings file, if any.</param>
    /// <param name="Threshold">The slow phase threshold in milliseconds, if any.</param>
    internal readonly record struct AssemblyMetadataValues(string? Enabled, string? File, string? Threshold)
    {
        /// <summary>
        /// Reads the values from the loaded assemblies that are not part of a framework.
        /// </summary>
        /// <returns>The values.</returns>
        public static AssemblyMetadataValues Read()
        {
            var values = default(AssemblyMetadataValues);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic || IsFramework(assembly.GetName().Name)) continue;
                foreach (var metadata in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
                {
                    values = values.With(metadata);
                }
            }

            return values;
        }

        AssemblyMetadataValues With(AssemblyMetadataAttribute metadata) => metadata.Key switch
        {
            EnabledKey => this with { Enabled = metadata.Value },
            FileKey => this with { File = metadata.Value },
            ThresholdKey => this with { Threshold = metadata.Value },
            _ => this
        };
    }

    static bool IsTrue(string? value) => value is not null && (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));

    static bool IsFramework(string? name)
    {
        if (name is null) return true;
        if (Array.IndexOf(_frameworkNames, name) >= 0) return true;
        foreach (var prefix in _frameworkPrefixes)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }
}
