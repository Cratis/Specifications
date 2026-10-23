// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Defines the style of a printed line.
/// </summary>
internal enum LineStyle
{
    /// <summary>No color.</summary>
    Plain,

    /// <summary>A passed fact.</summary>
    Passed,

    /// <summary>A failed fact or context.</summary>
    Failed,

    /// <summary>A skipped fact.</summary>
    Skipped,

    /// <summary>Secondary information such as locations and phases.</summary>
    Dim
}

/// <summary>
/// Represents a line printed by the console tree.
/// </summary>
/// <param name="Text">The text.</param>
/// <param name="Style">The style.</param>
internal readonly record struct OutputLine(string Text, LineStyle Style);
