// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// A change a fact makes to state its specification shares between facts.
/// </summary>
/// <param name="member">The shared field or property that changes.</param>
/// <param name="helper">The specification method the fact changes it through, or null when the fact changes it itself.</param>
sealed class SharedStateChange(string member, string? helper)
{
    /// <summary>
    /// Gets the shared field or property that changes.
    /// </summary>
    public string Member => member;

    /// <summary>
    /// Gets how the fact reaches the change, for the diagnostic message.
    /// </summary>
    public string Through => helper is null ? string.Empty : $" through '{helper}()'";
}
