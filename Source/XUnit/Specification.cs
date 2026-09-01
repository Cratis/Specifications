// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit;

namespace Cratis.Specifications;

/// <summary>
/// Represents the base class for specifications.
/// </summary>
/// <remarks>
/// The lifecycle of a specification is as follows:
/// - Establish : establishes the context (Given)
/// - Because : performs the action       (When)
/// - [Fact] : all your facts             (Then)
/// - Destroy : performs cleanup of the context
/// .
/// The different methods are by convention, meaning that having a private method called "Establish", "Destroy", "Because"
/// with a void signature taking no arguments will automatically be called.
/// All "Then" statements are considered the xUnit Facts we want to run assertions for.
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
///     [Fact] It_should_do_something() => ....
/// }
/// .
/// It will run the Establish first for the `a_specific_context` and then the `when_doing_something`
/// class.
/// </remarks>
public class Specification : IAsyncLifetime
{
    bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification"/> class.
    /// </summary>
    public Specification()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the instance is shared across all facts in its class.
    /// </summary>
    /// <remarks>
    /// Set by <see cref="SpecificationTestClassRunner"/> when the opt-in shared lifecycle is active.
    /// When set, <see cref="DisposeAsync"/> becomes a no-op so the per-fact lifecycle applied by
    /// the stock xUnit invoker does not destroy the context between facts; the class runner instead
    /// calls <see cref="DestroyAsync"/> once after the last fact.
    /// </remarks>
    internal bool IsSharedInstance { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// Establish and Because run only on the first call; subsequent calls on the same instance are
    /// no-ops. Stock xUnit calls this exactly once per instance, so this only has an effect when the
    /// instance is shared across facts through <see cref="SpecificationTestFramework"/>.
    /// </remarks>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        await OnEstablish();
        await OnBecause();
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (IsSharedInstance) return;

        await OnDestroy();
    }

    /// <summary>
    /// Performs the Destroy lifecycle for an instance shared across all facts in its class.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal Task DestroyAsync() => OnDestroy();

    Task OnEstablish()
    {
        return InvokeMethod("Establish");
    }

    Task OnBecause()
    {
        return InvokeMethod("Because");
    }

    Task OnDestroy()
    {
        return InvokeMethod("Destroy");
    }

    Task InvokeMethod(string name)
    {
#nullable disable
        return typeof(SpecificationMethods<,>).MakeGenericType(GetType(), typeof(Specification)).GetMethod(name, BindingFlags.Static | BindingFlags.Public).Invoke(null, [this]) as Task;
    }
}
