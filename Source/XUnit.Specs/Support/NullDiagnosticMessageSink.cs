// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;

namespace Cratis.Specifications.Support;

/// <summary>
/// A message sink that ignores every diagnostic message.
/// </summary>
public class NullDiagnosticMessageSink : Xunit.LongLivedMarshalByRefObject, IMessageSink
{
    /// <inheritdoc/>
    public bool OnMessage(IMessageSinkMessage message) => true;
}
