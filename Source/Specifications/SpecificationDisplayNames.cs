// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text;

namespace Cratis.Specifications;

/// <summary>
/// Derives readable Given/When/Then names from the namespace and type hierarchy of a specification.
/// </summary>
/// <remarks>
/// for_Author.when_registering.and_the_name_is_taken.should_fail reads "Author › when registering › and the name is taken › should fail":
/// the namespace from the first for_ segment, the subject without its for_ prefix, and underscores as spaces.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
#if SPECIFICATIONS_GENERATOR
internal
#else
public
#endif
static class SpecificationDisplayNames
{
    /// <summary>
    /// The separator between the levels of the hierarchy.
    /// </summary>
    public const string Separator = " \u203A ";

    /// <summary>
    /// Creates the display name of a specification class.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="typeNames">The type name and the names of its containing types, outermost first.</param>
    /// <returns>The display name.</returns>
    public static string ForClass(string @namespace, IEnumerable<string> typeNames)
    {
        var segments = @namespace.Length == 0 ? [] : @namespace.Split('.');
        var start = Array.FindIndex(segments, _ => _.StartsWith("for_", StringComparison.Ordinal));
        var levels = new List<string>();
        if (start >= 0)
        {
            levels.Add(Readable(segments[start].Substring(4)));
            for (var index = start + 1; index < segments.Length; index++) levels.Add(Readable(segments[index]));
        }

        levels.AddRange(typeNames.Select(Readable));
        return string.Join(Separator, levels);
    }

    /// <summary>
    /// Creates a display name from a runtime specification type, including containing types.
    /// </summary>
    /// <param name="type">The specification type.</param>
    /// <returns>The readable class name.</returns>
    public static string ForType(Type type)
    {
        var names = new List<string>();
        for (var current = type; current is not null; current = current.DeclaringType) names.Insert(0, current.Name);
        return ForClass(type.Namespace ?? string.Empty, names);
    }

    /// <summary>
    /// Creates the display name of a fact.
    /// </summary>
    /// <param name="classDisplayName">The display name of the specification class.</param>
    /// <param name="methodName">The name of the fact method.</param>
    /// <returns>The display name.</returns>
    public static string ForFact(string classDisplayName, string methodName) => classDisplayName + Separator + Readable(methodName);

    /// <summary>
    /// Turns an identifier into words: underscores become spaces.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <returns>The readable words.</returns>
    public static string Readable(string identifier)
    {
        var builder = new StringBuilder(identifier.Length);
        var lastWasSpace = true;
        foreach (var character in identifier.Trim('_'))
        {
            if (character == '_')
            {
                if (!lastWasSpace) builder.Append(' ');
                lastWasSpace = true;
            }
            else
            {
                builder.Append(character);
                lastWasSpace = character == ' ';
            }
        }

        return builder.ToString();
    }
}
