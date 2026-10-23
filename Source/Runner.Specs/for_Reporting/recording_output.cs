// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

internal sealed class recording_output : ISpecificationOutput
{
    public List<OutputLine> Lines { get; } = [];

    public bool UsedColor { get; private set; }

    public Task Write(IReadOnlyList<OutputLine> lines, bool useColor)
    {
        Lines.AddRange(lines);
        UsedColor |= useColor;
        return Task.CompletedTask;
    }
}
