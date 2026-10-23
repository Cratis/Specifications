// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.Generator;

/// <summary>
/// Derives readable Given/When/Then names from the namespace and type hierarchy of a specification.
/// </summary>
/// <remarks>
/// for_Author.when_registering.and_the_name_is_taken.should_fail reads "Author › when registering › and the name is taken › should fail":
/// the namespace from the first for_ segment, the subject without its for_ prefix, and underscores as spaces.
/// </remarks>
public static class DisplayNames
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
        return SpecificationDisplayNames.ForClass(@namespace, typeNames);
    }

    /// <summary>
    /// Creates the display name of a fact.
    /// </summary>
    /// <param name="classDisplayName">The display name of the specification class.</param>
    /// <param name="methodName">The name of the fact method.</param>
    /// <returns>The display name.</returns>
    public static string ForFact(string classDisplayName, string methodName) => classDisplayName + Separator + Readable(methodName);

    /// <summary>
    /// Gives a specification the raw display names used when readable names are off: the qualified type name for the class
    /// and the method name for the fact, still joined by <see cref="Separator"/> so reports can split them.
    /// </summary>
    /// <param name="model">The specification model with readable names.</param>
    /// <returns>The model with raw names.</returns>
    public static SpecificationModel Raw(SpecificationModel model) => model with
    {
        DisplayName = model.Uid,
        Facts = new([.. model.Facts.Select(_ => _ with { DisplayName = model.Uid + Separator + _.Call.Name })])
    };

    /// <summary>
    /// Turns an identifier into words: underscores become spaces.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <returns>The readable words.</returns>
    public static string Readable(string identifier)
    {
        return SpecificationDisplayNames.Readable(identifier);
    }
}
