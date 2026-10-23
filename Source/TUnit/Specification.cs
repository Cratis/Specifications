// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Represents the base class for specifications.
/// </summary>
/// <remarks>
/// The lifecycle of a specification is as follows:
/// - Establish : establishes the context (Given)
/// - Because : performs the action       (When)
/// - [Test] : all your facts             (Then)
/// - Destroy : performs cleanup of the context
/// .
/// The different methods are by convention, meaning that having a private method called "Establish", "Destroy", "Because"
/// with a void or Task signature taking no arguments will automatically be called.
/// All "Then" statements are the TUnit tests we want to run assertions for.
/// .
/// Establish and Because run once per specification class, before the first fact, and Destroy runs once after the last fact.
/// All facts of a class share the same instance, and therefore the same context. If Establish or Because fails, every fact fails.
/// Facts of a class may run in parallel, so they should only read from the context.
/// .
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
/// .
/// Specification class:
/// _
/// public class when_doing_something : a_specific_context
/// {
///     void Establish() => ....
///     void Because() => ....
///     [Test] public void should_do_something() => ....
/// }
/// .
/// It will run the Establish first for the `a_specific_context` and then the `when_doing_something`
/// class.
/// </remarks>
[SpecificationDisplayName]
[ClassConstructor<SpecificationClassConstructor>]
public class Specification
{
    readonly Lock _lock = new();
    Task? _initialization;

    /// <summary>
    /// Establishes the context and performs the action. Runs the lifecycle once, no matter how many facts there are.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Before(HookType.Test)]
    public Task InitializeAsync()
    {
        lock (_lock)
        {
            return _initialization ??= EstablishAndPerformAction();
        }
    }

    /// <summary>
    /// Destroys the context of the specification, once all its facts have completed.
    /// </summary>
    /// <param name="context">The context of the class that was run.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [After(HookType.Class)]
    public static Task DisposeAsync(ClassHookContext context) => DestroyAsync(context.ClassType);

    /// <summary>
    /// Destroys the shared instance of the given specification class, if its context was ever established.
    /// </summary>
    /// <param name="type">The specification class.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task DestroyAsync(Type type)
    {
        if (SpecificationClassConstructor.Release(type) is Specification { _initialization: not null } specification)
        {
            await specification.OnDestroy();
        }
    }

    async Task EstablishAndPerformAction()
    {
        await SpecificationLifecycle.Establish(this, GetType(), typeof(Specification));
        await SpecificationLifecycle.Because(this, GetType(), typeof(Specification));
    }

    Task OnDestroy() => SpecificationLifecycle.Destroy(this, GetType(), typeof(Specification));
}
