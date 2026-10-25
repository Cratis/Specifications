// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Identifies the separate teardown result of a specification class.
/// </summary>
internal static class TearDownNodeUid
{
    /// <summary>
    /// Gets the stable UID used both for reporting and rerunning class teardown failures.
    /// </summary>
    /// <param name="specification">The owning specification class.</param>
    /// <returns>The teardown node UID.</returns>
    public static string For(SpecificationClassDescriptor specification) => specification.Uid + ".(teardown)";
}
