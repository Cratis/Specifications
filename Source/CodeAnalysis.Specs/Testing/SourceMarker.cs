// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Cratis.Specifications.CodeAnalysis.Specs.Testing;

/// <summary>
/// Parses marker spans from test source strings. A marker looks like <c>{|#0:marked text|}</c>.
/// </summary>
public static class SourceMarker
{
    const string MarkerStart = "{|#";
    const char MarkerSeparator = ':';
    const string MarkerEnd = "|}";

    /// <summary>
    /// Parse the source and extract marker spans while returning cleaned source.
    /// </summary>
    /// <param name="source">The marked source string.</param>
    /// <returns>The cleaned source and marker span map.</returns>
    public static MarkedSource Parse(string source)
    {
        var markers = new Dictionary<int, TextSpan>();
        var builder = new StringBuilder();
        var currentIndex = 0;

        while (true)
        {
            var startIndex = source.IndexOf(MarkerStart, currentIndex, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                builder.Append(source.Substring(currentIndex));
                break;
            }

            builder.Append(source.Substring(currentIndex, startIndex - currentIndex));

            var idStart = startIndex + MarkerStart.Length;
            var separatorIndex = source.IndexOf(MarkerSeparator, idStart);
            if (separatorIndex < 0)
            {
                break;
            }

            var idText = source.Substring(idStart, separatorIndex - idStart);
            if (!int.TryParse(idText, out var markerId))
            {
                break;
            }

            var contentStart = separatorIndex + 1;
            var endIndex = source.IndexOf(MarkerEnd, contentStart, StringComparison.Ordinal);
            if (endIndex < 0)
            {
                break;
            }

            var content = source.Substring(contentStart, endIndex - contentStart);
            var spanStart = builder.Length;
            builder.Append(content);

            markers[markerId] = new TextSpan(spanStart, content.Length);
            currentIndex = endIndex + MarkerEnd.Length;
        }

        return new MarkedSource(builder.ToString(), markers);
    }
}
