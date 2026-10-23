// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Defines where the console tree prints.
/// </summary>
internal interface ISpecificationOutput
{
    /// <summary>
    /// Prints lines.
    /// </summary>
    /// <param name="lines">The lines.</param>
    /// <param name="useColor">Whether to color the lines.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Write(IReadOnlyList<OutputLine> lines, bool useColor);
}

/// <summary>
/// Prints through the Microsoft.Testing.Platform output device.
/// </summary>
/// <param name="device">The output device.</param>
internal sealed class OutputDeviceSpecificationOutput(IOutputDevice device) : ISpecificationOutput, IOutputDeviceDataProducer
{
    /// <inheritdoc/>
    public string Uid => nameof(OutputDeviceSpecificationOutput);

    /// <inheritdoc/>
    public string Version => "1.0.0";

    /// <inheritdoc/>
    public string DisplayName => "Cratis Specifications console tree";

    /// <inheritdoc/>
    public string Description => "Prints the specifications grouped by their names.";

    /// <inheritdoc/>
    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    /// <inheritdoc/>
    public async Task Write(IReadOnlyList<OutputLine> lines, bool useColor)
    {
        foreach (var line in lines)
        {
            var color = useColor ? ColorOf(line.Style) : null;
            var data = color is { } consoleColor
                ? new FormattedTextOutputDeviceData(line.Text) { ForegroundColor = new SystemConsoleColor { ConsoleColor = consoleColor } }
                : new FormattedTextOutputDeviceData(line.Text);
            await device.DisplayAsync(this, data, CancellationToken.None);
        }
    }

    static ConsoleColor? ColorOf(LineStyle style) => style switch
    {
        LineStyle.Passed => ConsoleColor.Green,
        LineStyle.Failed => ConsoleColor.Red,
        LineStyle.Skipped => ConsoleColor.Yellow,
        LineStyle.Dim => ConsoleColor.DarkGray,
        _ => null
    };
}
