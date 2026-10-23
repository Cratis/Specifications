// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_RunCache;

public class when_saving_and_loading : Specification
{
    string path;
    RunCache loaded;

    void Establish()
    {
        path = Path.Combine(Path.GetTempPath(), $"specs-cache-{Guid.NewGuid():N}");
        var first = RunCache.Empty;
        first.Record("kept", TimeSpan.FromMilliseconds(12.5), true);
        first.Record("replaced", TimeSpan.FromMilliseconds(1), true);
        first.Save(path);
        var second = RunCache.Load(path);
        second.Record("replaced", TimeSpan.FromMilliseconds(3), false);
        second.Save(path);
    }

    void Because() => loaded = RunCache.Load(path);

    void Destroy() => File.Delete(path);

    void should_keep_the_classes_that_did_not_run() => loaded.TryGet("kept", out _).ShouldBeTrue();

    void should_keep_the_duration_and_failure() => (loaded.TryGet("kept", out var entry) ? entry : default).ShouldEqual(new RunCacheEntry(12.5, true));

    void should_replace_the_classes_that_ran() => (loaded.TryGet("replaced", out var entry) ? entry : default).ShouldEqual(new RunCacheEntry(3, false));

    void should_give_an_empty_cache_for_a_missing_file() => RunCache.Load(path + ".missing").HasHistory.ShouldBeFalse();
}
