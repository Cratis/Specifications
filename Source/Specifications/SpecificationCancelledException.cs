// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// The exception that is thrown when a specification lifecycle method (Establish, Because, or Destroy)
/// is cancelled before it completes — typically because a polling wait exceeded its timeout.
/// </summary>
/// <param name="phase">The lifecycle phase that was cancelled (Establish, Because, or Destroy).</param>
/// <param name="innerException">The original cancellation exception.</param>
public class SpecificationCancelledException(string phase, OperationCanceledException innerException)
    : Exception(
        $"{phase}() was cancelled before completing. This usually means a polling wait " +
        "(e.g. WaitTillActive, WaitForState, WaitTillReachesEventSequenceNumber) exceeded its timeout. " +
        "Consider increasing the timeout via CHRONICLE_TEST_TIMEOUT_SECONDS or passing an explicit timeout.",
        innerException)
{
}
