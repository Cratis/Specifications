// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.Runner.Generator;

/// <summary>
/// Builds the model of a specification class from its symbol.
/// </summary>
public static class SpecificationModelBuilder
{
    const string SpecificationType = "Cratis.Specifications.Specification";
    const string RunnerAssembly = "Cratis.Specifications.Runner";
    const string FactAttribute = "Cratis.Specifications.FactAttribute";

    static readonly SymbolDisplayFormat _qualified = SymbolDisplayFormat.FullyQualifiedFormat;

    /// <summary>
    /// Builds the model of a class, if it is a runnable specification with at least one fact.
    /// </summary>
    /// <param name="type">The class.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The model, or null if the class is not a runnable specification.</returns>
    public static SpecificationModel? Build(INamedTypeSymbol type, Compilation compilation)
    {
        if (type.IsAbstract || type.IsStatic || type.IsGenericType || IsInGenericType(type) || !IsSpecification(type) ||
            !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            return null;
        }

        var chain = Chain(type);
        var errors = new List<string>();
        var diagnostics = new List<DiagnosticModel>();
        var facts = Facts(chain, compilation, errors, diagnostics);
        if (facts.Count == 0 && errors.Count == 0) return null;

        var establish = Lifecycle(chain, compilation, errors, diagnostics, "Establish", "Given");
        var because = Lifecycle(chain, compilation, errors, diagnostics, "Because", "When");
        var destroy = Lifecycle(chain, compilation, errors, diagnostics, "Destroy", null);
        var constructor = type.InstanceConstructors.FirstOrDefault(_ => _.Parameters.Length == 0);
        if (constructor is null) errors.Add($"{type.Name} has no parameterless constructor.");

        var @namespace = type.ContainingNamespace.IsGlobalNamespace ? string.Empty : type.ContainingNamespace.ToDisplayString();
        var typeNames = TypeNames(type);
        var typeName = string.Join("+", typeNames);
        var display = DisplayNames.ForClass(@namespace, typeNames);
        return new(
            @namespace.Length == 0 ? typeName : $"{@namespace}.{typeName}",
            @namespace,
            typeName,
            display,
            type.ToDisplayString(_qualified),
            constructor is not null && compilation.IsSymbolAccessibleWithin(constructor, compilation.Assembly),
            establish,
            because,
            destroy,
            new([.. facts.Select(_ => _ with { DisplayName = DisplayNames.ForFact(display, _.Call.Name) })]),
            errors.Count == 0 ? null : string.Join(" ", errors),
            new([.. diagnostics]));
    }

    static bool IsSpecification(INamedTypeSymbol type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == SpecificationType && current.ContainingAssembly?.Name == RunnerAssembly) return true;
        }

        return false;
    }

    static bool IsInGenericType(INamedTypeSymbol type)
    {
        for (var current = type.ContainingType; current is not null; current = current.ContainingType)
        {
            if (current.IsGenericType) return true;
        }

        return false;
    }

    static List<string> TypeNames(INamedTypeSymbol type)
    {
        var names = new List<string>();
        for (var current = type; current is not null; current = current.ContainingType) names.Insert(0, current.Name);
        return names;
    }

    static List<INamedTypeSymbol> Chain(INamedTypeSymbol type)
    {
        var chain = new List<INamedTypeSymbol>();
        for (var current = type; current is not null && current.ToDisplayString() != SpecificationType; current = current.BaseType)
        {
            chain.Insert(0, current);
        }

        return chain;
    }

    static EquatableArray<MethodCall> Lifecycle(List<INamedTypeSymbol> chain, Compilation compilation, List<string> errors, List<DiagnosticModel> diagnostics, string name, string? alias)
    {
        var calls = ImmutableArray.CreateBuilder<MethodCall>();
        foreach (var type in chain)
        {
            var primary = Find(type, name);
            var secondary = alias is null ? null : Find(type, alias);
            if (primary is not null && secondary is not null)
            {
                errors.Add($"{type.Name} declares both {name} and {alias}; use one of them.");
                continue;
            }

            if ((primary ?? secondary) is IMethodSymbol method && ToCall(method, compilation, errors, diagnostics) is MethodCall call) calls.Add(call);
        }

        return new(calls.ToImmutable());
    }

    static IMethodSymbol? Find(INamedTypeSymbol type, string name) =>
        type.GetMembers(name).OfType<IMethodSymbol>().FirstOrDefault(_ => !_.IsStatic && !_.IsOverride && _.Parameters.Length == 0 && !_.IsGenericMethod);

    static List<FactModel> Facts(List<INamedTypeSymbol> chain, Compilation compilation, List<string> errors, List<DiagnosticModel> diagnostics)
    {
        var facts = new List<FactModel>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = chain.Count - 1; index >= 0; index--)
        {
            var own = new List<FactModel>();
            foreach (var method in chain[index].GetMembers().OfType<IMethodSymbol>())
            {
                var attribute = method.GetAttributes().FirstOrDefault(_ => _.AttributeClass?.ToDisplayString() == FactAttribute);
                if (method.MethodKind != MethodKind.Ordinary || method.IsStatic || method.IsGenericMethod || method.Parameters.Length > 0 ||
                    (attribute is null && !method.Name.StartsWith("should_", StringComparison.Ordinal)) || !seen.Add(method.Name) ||
                    ToFactCall(method, compilation, errors, diagnostics) is not MethodCall call)
                {
                    continue;
                }

                var location = method.Locations.FirstOrDefault(_ => _.IsInSource)?.GetLineSpan();
                var skip = attribute?.NamedArguments.FirstOrDefault(_ => _.Key == "Skip").Value.Value as string;
                own.Add(new(call, method.Name, location?.Path, (location?.StartLinePosition.Line ?? -1) + 1, skip));
            }

            facts.InsertRange(0, own);
        }

        return facts;
    }

    // A fact with an unsupported return type fails its class with a discovery error. It still needs a call to be listed under, so it gets one
    // through reflection that never runs, because the error fails every fact of the class before anything is invoked.
    static MethodCall? ToFactCall(IMethodSymbol method, Compilation compilation, List<string> errors, List<DiagnosticModel> diagnostics)
    {
        if (ToCall(method, compilation, errors, diagnostics) is MethodCall call) return call;
        return new(method.ContainingType.ToDisplayString(_qualified), method.Name, ReturnKind.Void, CallKind.Reflection);
    }

    static MethodCall? ToCall(IMethodSymbol method, Compilation compilation, List<string> errors, List<DiagnosticModel> diagnostics)
    {
        if (method.IsAsync && method.ReturnsVoid) diagnostics.Add(Diagnostics.For(Diagnostics.AsyncVoidId, method));

        ReturnKind? returns = method.ReturnsVoid ? ReturnKind.Void : method.ReturnType.ToDisplayString() switch
        {
            "System.Threading.Tasks.Task" => ReturnKind.Task,
            "System.Threading.Tasks.ValueTask" => ReturnKind.ValueTask,
            _ => null
        };

        if (returns is null)
        {
            errors.Add($"{method.ContainingType.Name}.{method.Name} must return void, Task or ValueTask.");
            return null;
        }

        var declaring = method.ContainingType;
        var how = compilation.IsSymbolAccessibleWithin(method, compilation.Assembly) ? CallKind.Direct
            : declaring.IsGenericType ? CallKind.Reflection
            : CallKind.Accessor;
        if (how == CallKind.Reflection) diagnostics.Add(Diagnostics.For(Diagnostics.ReflectionFallbackId, method));
        return new(declaring.ToDisplayString(_qualified), method.Name, returns.Value, how);
    }
}
