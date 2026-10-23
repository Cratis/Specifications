// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Creates the exception thrown when a "Should*" assertion does not hold, in the shape the test framework in use expects.
/// </summary>
/// <remarks>
/// The xUnit and NUnit packages register their factory from a module initializer when their assembly loads, which keeps the
/// lookup free of reflection and so trim and AOT safe. When no package has registered one (TUnit and the native runner) the
/// failure is a <see cref="SpecificationAssertionException"/>.
/// </remarks>
static class AssertionFailures
{
    static readonly SpecificationAssertionFailureFactory _default = new();
    static volatile IAssertionFailureFactory? _factory;

    /// <summary>
    /// Creates the exception describing a failed assertion.
    /// </summary>
    /// <param name="message">The message describing the failed assertion.</param>
    /// <returns>The exception to throw.</returns>
    public static Exception Create(string message) =>
        AssertionValues.TryExtractExpectedActual(message, out var expected, out var actual)
            ? Create(message, expected, actual)
            : (_factory ?? _default).Create(message);

    /// <summary>
    /// Creates the exception describing a failed assertion that compared an expected and an actual value.
    /// </summary>
    /// <param name="message">The message describing the failed assertion.</param>
    /// <param name="expected">The formatted expected value.</param>
    /// <param name="actual">The formatted actual value.</param>
    /// <returns>The exception to throw.</returns>
    /// <remarks>
    /// Only the failure of the native runner and TUnit carries the values; xUnit and NUnit keep their own exception types and the formatted message.
    /// </remarks>
    public static Exception Create(string message, string? expected, string? actual)
    {
        var factory = _factory ?? _default;
        return factory is SpecificationAssertionFailureFactory
            ? new SpecificationAssertionException(message, expected, actual)
            : factory.Create(message);
    }

    /// <summary>
    /// Registers the factory a package uses to create assertion failures in the shape its test framework expects.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <remarks>
    /// Called by the module initializer of a package, so the first failed assertion does no probing for assemblies.
    /// </remarks>
    internal static void Register(IAssertionFailureFactory factory) => _factory = factory;

    sealed class SpecificationAssertionFailureFactory : IAssertionFailureFactory
    {
        public Exception Create(string message) => new SpecificationAssertionException(message);
    }
}
