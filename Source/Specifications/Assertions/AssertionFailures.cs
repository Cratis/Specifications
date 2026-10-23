// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Creates the exception thrown when a "Should*" assertion does not hold, in the shape the test framework in use expects.
/// </summary>
/// <remarks>
/// The factory is resolved once, on the first failed assertion, so a passing assertion never pays for it. Resolution works the same
/// whether or not the asserting class derives from a package's Specification: the assertions live in this assembly, so the package
/// assembly may never have been loaded. Each package name is looked up with a constant string, which keeps the lookup trim and AOT
/// analyzable; when no package is present (TUnit and the native runner) the failure is a <see cref="SpecificationAssertionException"/>.
/// </remarks>
static class AssertionFailures
{
    static IAssertionFailureFactory? _factory;

    /// <summary>
    /// Creates the exception describing a failed assertion.
    /// </summary>
    /// <param name="message">The message describing the failed assertion.</param>
    /// <returns>The exception to throw.</returns>
    public static Exception Create(string message) =>
        AssertionValues.TryExtractExpectedActual(message, out var expected, out var actual)
            ? Create(message, expected, actual)
            : (_factory ??= Resolve()).Create(message);

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
        var factory = _factory ??= Resolve();
        return factory is SpecificationAssertionFailureFactory
            ? new SpecificationAssertionException(message, expected, actual)
            : factory.Create(message);
    }

    static IAssertionFailureFactory Resolve()
    {
        var factoryType =
            Type.GetType("Cratis.Specifications.AssertionFailureFactory, Cratis.Specifications.XUnit", throwOnError: false) ??
            Type.GetType("Cratis.Specifications.AssertionFailureFactory, Cratis.Specifications.XUnit.v3", throwOnError: false) ??
            Type.GetType("Cratis.Specifications.AssertionFailureFactory, Cratis.Specifications.NUnit", throwOnError: false) ??
            Type.GetType("Cratis.Specifications.AssertionFailureFactory, Cratis.Specifications.NUnit5", throwOnError: false);

        return factoryType is not null && Activator.CreateInstance(factoryType) is IAssertionFailureFactory factory
            ? factory
            : new SpecificationAssertionFailureFactory();
    }

    sealed class SpecificationAssertionFailureFactory : IAssertionFailureFactory
    {
        public Exception Create(string message) => new SpecificationAssertionException(message);
    }
}
