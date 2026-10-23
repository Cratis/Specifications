// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

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
[TestFixture]
[SpecificationDisplayName]
public class Specification
{
    SpecificationScope? _scope;

    [OneTimeSetUp]
    public async Task InitializeAsync()
    {
        _scope = SpecificationScope.CreateIfObserved(this);
        if (_scope is not null) await _scope.Start();
        await OnEstablish();
        await OnBecause();
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        try
        {
            await OnDestroy();
        }
        finally
        {
            if (_scope is not null) await _scope.End();
        }
    }

    Task OnEstablish() => _scope?.Establish(typeof(Specification)) ?? SpecificationLifecycle.Establish(this, GetType(), typeof(Specification));

    Task OnBecause() => _scope?.Because(typeof(Specification)) ?? SpecificationLifecycle.Because(this, GetType(), typeof(Specification));

    Task OnDestroy() => _scope?.Destroy(typeof(Specification)) ?? SpecificationLifecycle.Destroy(this, GetType(), typeof(Specification));
}

/// <summary>
/// Applies readable names only when the test assembly opts in.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class SpecificationDisplayNameAttribute : NUnitAttribute, IApplyToTest
{
    /// <inheritdoc/>
    public void ApplyToTest(Test test)
    {
        var type = test.TypeInfo?.Type;
        if (type is null) return;
        if (!typeof(Specification).IsAssignableFrom(type)) return;
        if (!type.Assembly.IsDefined(typeof(ReadableSpecificationNamesAttribute))) return;
        ApplyNames(test, SpecificationDisplayNames.ForType(type));
    }

    static void ApplyNames(Test test, string className)
    {
        var suffix = test.Name.IndexOf('(');
        test.Name = (test.Method is null ? className : SpecificationDisplayNames.ForFact(className, test.Method.Name))
            + (suffix < 0 ? string.Empty : test.Name.Substring(suffix));
        foreach (var child in test.Tests.OfType<Test>()) ApplyNames(child, className);
    }
}
