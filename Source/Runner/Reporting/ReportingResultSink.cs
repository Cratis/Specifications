// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Decorates a result sink with the console tree and the reports. It only exists when one of them is wanted, so a run without
/// either (quiet, or server mode without reports) pays nothing per class or per fact.
/// </summary>
internal sealed class ReportingResultSink : ISpecificationResultSink, IDisposable
{
    readonly ISpecificationResultSink _inner;
    readonly SpecificationReportOptions _options;
    readonly ISpecificationOutput? _output;
    readonly ConsoleTreeRenderer? _renderer;
    readonly ReportTree? _tree;
    readonly Func<string, string?> _environment;
    readonly SemaphoreSlim _gate = new(1, 1);

    ReportingResultSink(ISpecificationResultSink inner, SpecificationReportOptions options, ISpecificationOutput? output, bool console, Func<string, string?> environment)
    {
        _inner = inner;
        _options = options;
        _environment = environment;
        if (console)
        {
            _output = output;
            _renderer = new ConsoleTreeRenderer(options);
        }

        if (options.Reports.Count > 0) _tree = new ReportTree();
        _output ??= output;
    }

    /// <summary>
    /// Creates the decorator when the console tree or a report is wanted.
    /// </summary>
    /// <param name="inner">The sink to decorate.</param>
    /// <param name="options">The report options.</param>
    /// <param name="server">Whether the run is driven by an IDE (server mode), which shows results itself.</param>
    /// <param name="output">Where the console tree prints; null when there is no output device.</param>
    /// <param name="environment">Reads an environment variable.</param>
    /// <returns>The decorator, or null when nothing is wanted.</returns>
    public static ReportingResultSink? Create(ISpecificationResultSink inner, SpecificationReportOptions options, bool server, ISpecificationOutput? output, Func<string, string?> environment)
    {
        var console = options.WantsConsole && !server && output is not null;
        return console || options.Reports.Count > 0 ? new ReportingResultSink(inner, options, output, console, environment) : null;
    }

    /// <inheritdoc/>
    public ValueTask Starting(SpecificationWork work) => _inner.Starting(work);

    /// <inheritdoc/>
    public async ValueTask Completed(SpecificationWork work, IReadOnlyList<FactResult> results, FactResult? teardownFailure = null)
    {
        await _inner.Completed(work, results, teardownFailure);
        await Report(ClassReport.From(work, results, teardownFailure));
    }

    /// <summary>
    /// Reports a class that did not run because the run stopped.
    /// </summary>
    /// <param name="work">The work that did not run.</param>
    /// <param name="reason">Why it did not run.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task NotRun(SpecificationWork work, string reason)
    {
        var results = new FactResult[work.Facts.Count];
        for (var index = 0; index < results.Length; index++) results[index] = FactResult.Skipped(reason, DateTimeOffset.UtcNow);
        await Report(ClassReport.From(work, results));
    }

    /// <summary>
    /// Writes the requested reports.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Finish()
    {
        if (_tree is null) return;
        var problems = ReportWriter.Write(_options.Reports, _tree, _environment);
        if (_output is null || problems.Count == 0) return;
        await _output.Write([.. problems.Select(_ => new OutputLine(_, LineStyle.Failed))], !_options.NoColor);
    }

    /// <inheritdoc/>
    public void Dispose() => _gate.Dispose();

    async ValueTask Report(ClassReport report)
    {
        _tree?.Add(report);
        if (_renderer is null) return;

        // Printing is additive; a failing output device must not fail the specifications.
        await _gate.WaitAsync();
        try
        {
            await _output!.Write(_renderer.Render(report), !_options.NoColor);
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException)
        {
            // Deliberately ignored.
        }
        finally
        {
            _gate.Release();
        }
    }
}
