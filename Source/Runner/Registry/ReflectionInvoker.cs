// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Invokes a lifecycle method or fact through reflection. Generated code falls back to this only where the
/// method cannot be reached through an UnsafeAccessor, such as a method declared on a constructed generic base type.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ReflectionInvoker
{
    /// <summary>
    /// Finds a parameterless instance method declared on the given type.
    /// </summary>
    /// <param name="type">The declaring type.</param>
    /// <param name="name">The method name.</param>
    /// <returns>The method.</returns>
    public static MethodInfo Find([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] Type type, string name) =>
        type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, Type.EmptyTypes)
            ?? throw new MissingMethodException(type.FullName, name);

    /// <summary>
    /// Invokes the method and adapts its result to a <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="method">The method to invoke.</param>
    /// <param name="instance">The instance to invoke it on.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public static ValueTask Invoke(MethodInfo method, object instance)
    {
        try
        {
            return method.Invoke(instance, null) switch
            {
                Task task => new ValueTask(task),
                ValueTask valueTask => valueTask,
                _ => default
            };
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}
