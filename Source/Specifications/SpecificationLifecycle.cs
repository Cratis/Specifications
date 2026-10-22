// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Cratis.Specifications;

/// <summary>
/// Runs the Establish, Because and Destroy lifecycle methods of a specification.
/// </summary>
/// <remarks>
/// The lifecycle methods of a type are discovered once and cached per specification type. They are found by
/// walking the inheritance chain up to (but excluding) the framework specification base type, looking only at
/// methods declared on each type, and are run from the most base type towards the most derived type.
/// A lifecycle method can be synchronous or return a <see cref="Task"/> or a <see cref="ValueTask"/>.
/// Exceptions thrown by the methods are surfaced as is, without being wrapped in a
/// <see cref="TargetInvocationException"/>, and cancellations are reported as a <see cref="SpecificationCancelledException"/>.
/// </remarks>
internal static class SpecificationLifecycle
{
    const string EstablishName = "Establish";
    const string BecauseName = "Because";
    const string DestroyName = "Destroy";

    static readonly ConcurrentDictionary<(Type Unit, Type Base), Methods> _cache = new();

    /// <summary>
    /// Run all Establish methods for a specification.
    /// </summary>
    /// <param name="unit">The specification instance.</param>
    /// <param name="unitType">The type whose inheritance chain holds the lifecycle methods.</param>
    /// <param name="specificationBase">The framework base type at which the inheritance walk stops.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task Establish(object unit, Type unitType, Type specificationBase) =>
        Run(GetMethods(unitType, specificationBase).Establish, unit, EstablishName);

    /// <summary>
    /// Run all Because methods for a specification.
    /// </summary>
    /// <param name="unit">The specification instance.</param>
    /// <param name="unitType">The type whose inheritance chain holds the lifecycle methods.</param>
    /// <param name="specificationBase">The framework base type at which the inheritance walk stops.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task Because(object unit, Type unitType, Type specificationBase) =>
        Run(GetMethods(unitType, specificationBase).Because, unit, BecauseName);

    /// <summary>
    /// Run all Destroy methods for a specification.
    /// </summary>
    /// <param name="unit">The specification instance.</param>
    /// <param name="unitType">The type whose inheritance chain holds the lifecycle methods.</param>
    /// <param name="specificationBase">The framework base type at which the inheritance walk stops.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task Destroy(object unit, Type unitType, Type specificationBase) =>
        Run(GetMethods(unitType, specificationBase).Destroy, unit, DestroyName);

    static Methods GetMethods(Type unit, Type specificationBase) =>
        _cache.GetOrAdd((unit, specificationBase), static key => new Methods(
            Discover(key.Unit, key.Base, EstablishName),
            Discover(key.Unit, key.Base, BecauseName),
            Discover(key.Unit, key.Base, DestroyName)));

    static MethodInfo[] Discover(Type unit, Type specificationBase, string name)
    {
        var methods = new List<MethodInfo>();
        var type = unit;

        while (type is not null && type != specificationBase)
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (method.Name == name && method.GetParameters().Length == 0 && !IsRunThroughBaseDeclaration(method, specificationBase))
                {
                    methods.Add(method);
                }
            }

            type = type.BaseType;
        }

        methods.Reverse();
        return [.. methods];
    }

    // An override is run through virtual dispatch from the method that introduced it; running it
    // separately as well would execute the same method twice.
    static bool IsRunThroughBaseDeclaration(MethodInfo method, Type specificationBase)
    {
        if (!method.IsVirtual) return false;

        var declaration = method.GetBaseDefinition().DeclaringType;
        return declaration is not null && declaration != method.DeclaringType && declaration != specificationBase && specificationBase.IsAssignableFrom(declaration);
    }

    static Task Run(MethodInfo[] methods, object unit, string phase)
    {
        if (methods.Length == 0) return Task.CompletedTask;

        try
        {
            for (var i = 0; i < methods.Length; i++)
            {
                var pending = Invoke(methods[i], unit, phase);
                if (pending is not null) return RunRemaining(pending, methods, i + 1, unit, phase);
            }
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }

        return Task.CompletedTask;
    }

    static async Task RunRemaining(Task pending, MethodInfo[] methods, int next, object unit, string phase)
    {
        await Await(pending, phase);

        for (; next < methods.Length; next++)
        {
            var task = Invoke(methods[next], unit, phase);
            if (task is not null) await Await(task, phase);
        }
    }

    static async Task Await(Task task, string phase)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException ex)
        {
            throw new SpecificationCancelledException(phase, ex);
        }
    }

    // Returns null when the method completed synchronously, otherwise the task to await.
    static Task? Invoke(MethodInfo method, object unit, string phase)
    {
        object? result;
        try
        {
            result = method.Invoke(unit, BindingFlags.DoNotWrapExceptions, null, null, null);
        }
        catch (OperationCanceledException ex)
        {
            throw new SpecificationCancelledException(phase, ex);
        }

        var task = result switch
        {
            Task t => t,
            ValueTask valueTask => valueTask.AsTask(),
            _ => AsTaskIfValueTaskOfT(result)
        };

        return task is null || task.IsCompletedSuccessfully ? null : task;
    }

    static Task? AsTaskIfValueTaskOfT(object? result)
    {
        var type = result?.GetType();
        if (type is null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ValueTask<>)) return null;
        return (Task?)type.GetMethod(nameof(ValueTask.AsTask), Type.EmptyTypes)!.Invoke(result, null);
    }

    sealed record Methods(MethodInfo[] Establish, MethodInfo[] Because, MethodInfo[] Destroy);
}
