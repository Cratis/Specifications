// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Represents the lifecycle methods for a specification.
/// </summary>
/// <typeparam name="T">Target type it represents.</typeparam>
/// <typeparam name="TSpecBase">The base type used for specifications.</typeparam>
/// <remarks>
/// It will recursively execute lifecycle methods in the inheritance hierarchy.
/// This enables one to encapsulate reusable contexts. The order it executes them in
/// is reversed; meaning that it will start at the lowest level in the inheritance chain
/// and move towards the specific type.
/// Example:
/// Context class:
/// public class a_specific_context : Specification
/// {
///     void Establish() => ....
/// }
/// _
/// Specification class:
/// _
/// public class when_doing_something : a_specific_context
/// {
///     void Establish() => ....
/// }
/// -
/// It will run the Establish first for the `a_specific_context` and then the `when_doing_something`
/// class.
/// </remarks>
public static class SpecificationMethods<T, TSpecBase>
{
    /// <summary>
    /// Invoke all Establish methods.
    /// </summary>
    /// <param name="unit">Unit to invoke them on.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task Establish(object unit) => SpecificationLifecycle.Establish(unit, typeof(T), typeof(TSpecBase));

    /// <summary>
    /// Invoke all Destroy methods.
    /// </summary>
    /// <param name="unit">Unit to invoke them on.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task Destroy(object unit) => SpecificationLifecycle.Destroy(unit, typeof(T), typeof(TSpecBase));

    /// <summary>
    /// Invoke all Because methods.
    /// </summary>
    /// <param name="unit">Unit to invoke them on.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task Because(object unit) => SpecificationLifecycle.Because(unit, typeof(T), typeof(TSpecBase));
}
