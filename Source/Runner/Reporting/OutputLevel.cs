// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Defines how much the console tree reporter prints.
/// </summary>
internal enum OutputLevel
{
    /// <summary>Nothing is printed and no reporter is registered.</summary>
    Quiet = 0,

    /// <summary>The tree of every fact with its outcome; failures show expected and actual values and the source location.</summary>
    Normal = 1,

    /// <summary>As normal, plus durations, phase steps and stack traces.</summary>
    Detailed = 2
}
