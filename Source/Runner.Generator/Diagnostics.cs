// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cratis.Specifications.Runner.Generator;

/// <summary>
/// Describes a diagnostic found while building the model of a specification, in a form that is comparable so the
/// model stays cacheable by the incremental generator.
/// </summary>
/// <param name="Id">The diagnostic id.</param>
/// <param name="Argument">The argument of the message.</param>
/// <param name="FilePath">The source file, if the diagnostic has a location.</param>
/// <param name="Span">The text span of the location.</param>
/// <param name="LineSpan">The line span of the location.</param>
public sealed record DiagnosticModel(string Id, string Argument, string? FilePath, TextSpan Span, LinePositionSpan LineSpan);

/// <summary>
/// The diagnostics reported by the specification generator.
/// </summary>
public static class Diagnostics
{
    const string Category = "Cratis.Specifications";

    /// <summary>
    /// The id of the diagnostic for an async void fact or lifecycle method.
    /// </summary>
    public const string AsyncVoidId = "CRSPEC0010";

    /// <summary>
    /// An async void fact or lifecycle method; its failures can not be observed so it can not be awaited.
    /// </summary>
    public static readonly DiagnosticDescriptor AsyncVoid = new(
        AsyncVoidId,
        "A fact or lifecycle method must not be async void",
        "{0} is async void, which can not be awaited and loses its failures; return Task or ValueTask instead",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// The id of the diagnostic for a method that is called through reflection.
    /// </summary>
    public const string ReflectionFallbackId = "CRSPEC0011";

    /// <summary>
    /// A method on a constructed generic base type that is not accessible to the test assembly is called through reflection, which is not AOT safe.
    /// </summary>
    public static readonly DiagnosticDescriptor ReflectionFallback = new(
        ReflectionFallbackId,
        "A method on a generic base type is called through reflection",
        "{0} is declared on a constructed generic type and is not accessible, so it is called through reflection, which is not trim or AOT safe; make it accessible to the test assembly",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Creates the model of a diagnostic located at a method.
    /// </summary>
    /// <param name="id">The diagnostic id.</param>
    /// <param name="method">The method the diagnostic is about.</param>
    /// <returns>The diagnostic model.</returns>
    public static DiagnosticModel For(string id, IMethodSymbol method)
    {
        var location = method.Locations.FirstOrDefault(_ => _.IsInSource);
        var lineSpan = location?.GetLineSpan();
        return new(id, $"{method.ContainingType.Name}.{method.Name}", lineSpan?.Path, location?.SourceSpan ?? default, lineSpan?.Span ?? default);
    }

    /// <summary>
    /// Reports a diagnostic model.
    /// </summary>
    /// <param name="context">The context to report to.</param>
    /// <param name="model">The diagnostic model.</param>
    public static void Report(SourceProductionContext context, DiagnosticModel model)
    {
        var location = model.FilePath is null ? Location.None : Location.Create(model.FilePath, model.Span, model.LineSpan);
        context.ReportDiagnostic(Diagnostic.Create(DescriptorFor(model.Id), location, model.Argument));
    }

    static DiagnosticDescriptor DescriptorFor(string id) => id switch
    {
        AsyncVoidId => AsyncVoid,
        ReflectionFallbackId => ReflectionFallback,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };
}
