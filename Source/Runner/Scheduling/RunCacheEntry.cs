// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents what an earlier run remembered about a specification class.
/// </summary>
/// <param name="Milliseconds">How long the class took.</param>
/// <param name="Failed">Whether any of its facts failed.</param>
internal readonly record struct RunCacheEntry(double Milliseconds, bool Failed);
