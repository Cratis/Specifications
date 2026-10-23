// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Cratis.Specifications;

/// <summary>
/// The entry point the session hooks compiled into NUnit and TUnit test projects call when the test session ends.
/// Not intended to be used directly.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class SpecificationSessionHook
{
    /// <summary>
    /// Ends the test session.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task End()
    {
        var end = SpecificationSession.End();
        return end.IsCompletedSuccessfully ? Task.CompletedTask : end.AsTask();
    }
}
