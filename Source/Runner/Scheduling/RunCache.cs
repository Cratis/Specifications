// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the small file remembering the duration and outcome of every specification class from earlier runs.
/// It drives longest-first and last-failed-first scheduling and duration-balanced sharding.
/// </summary>
/// <remarks>
/// The format is one line per class: the unique identifier, the duration in milliseconds and 1 if it failed, tab separated.
/// Entries of classes that were not run are kept, so a filtered or sharded run does not forget the rest.
/// </remarks>
internal sealed class RunCache
{
    readonly Dictionary<string, RunCacheEntry> _previous;
    readonly ConcurrentDictionary<string, RunCacheEntry> _current = new(StringComparer.Ordinal);

    RunCache(Dictionary<string, RunCacheEntry> previous) => _previous = previous;

    /// <summary>
    /// Gets an empty cache.
    /// </summary>
    public static RunCache Empty => new(new(StringComparer.Ordinal));

    /// <summary>
    /// Gets a value indicating whether there are any entries from earlier runs.
    /// </summary>
    public bool HasHistory => _previous.Count > 0;

    /// <summary>
    /// Loads the cache from a file; a missing or unreadable file gives an empty cache.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The cache.</returns>
    public static RunCache Load(string path)
    {
        var entries = new Dictionary<string, RunCacheEntry>(StringComparer.Ordinal);
        try
        {
            if (!File.Exists(path)) return new(entries);
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split('\t');
                if (parts.Length == 3 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds))
                {
                    entries[parts[0]] = new(milliseconds, parts[2] == "1");
                }
            }
        }
        catch (IOException)
        {
            entries.Clear();
        }

        return new(entries);
    }

    /// <summary>
    /// Gets the entry of a class from earlier runs.
    /// </summary>
    /// <param name="uid">The unique identifier of the class.</param>
    /// <param name="entry">The entry, if found.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGet(string uid, out RunCacheEntry entry) => _previous.TryGetValue(uid, out entry);

    /// <summary>
    /// Records the result of a class in this run.
    /// </summary>
    /// <param name="uid">The unique identifier of the class.</param>
    /// <param name="duration">How long the class took, from creation to disposal.</param>
    /// <param name="failed">Whether any of its facts failed.</param>
    public void Record(string uid, TimeSpan duration, bool failed) => _current[uid] = new(duration.TotalMilliseconds, failed);

    /// <summary>
    /// Saves the merged entries; failures to save are ignored, the cache is only an optimization.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    public void Save(string path)
    {
        var merged = new SortedDictionary<string, RunCacheEntry>(_previous, StringComparer.Ordinal);
        foreach (var (uid, entry) in _current)
        {
            merged[uid] = entry;
        }

        var builder = new StringBuilder();
        foreach (var (uid, entry) in merged)
        {
            builder.Append(uid).Append('\t')
                .Append(entry.Milliseconds.ToString("0.###", CultureInfo.InvariantCulture)).Append('\t')
                .Append(entry.Failed ? '1' : '0').Append('\n');
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, builder.ToString());
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
