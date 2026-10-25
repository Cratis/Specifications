// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Specifications.Runner.Generator;

/// <summary>
/// Emits the registration code of a specification class: a factory, the lifecycle chains and the facts as plain
/// delegates, with UnsafeAccessors for members the test assembly can not call directly. No reflection at run time.
/// </summary>
public static class SpecificationEmitter
{
    const string ValueTask = "global::System.Threading.Tasks.ValueTask";
    const string Runtime = "global::Cratis.Specifications.Runner";

    /// <summary>
    /// Emits the file local registration class of a specification.
    /// </summary>
    /// <param name="builder">The builder to emit to.</param>
    /// <param name="model">The specification.</param>
    /// <param name="className">The name of the registration class.</param>
    public static void Emit(StringBuilder builder, SpecificationModel model, string className)
    {
        var accessors = new StringBuilder();
        var counter = 0;
        builder.Append("    file static class ").Append(className).AppendLine();
        builder.AppendLine("    {");
        var body = new StringBuilder();
        var hasError = model.Error is not null;
        var establish = hasError ? null : Chain(body, accessors, model, model.Establish, "Establish", ref counter);
        var because = hasError ? null : Chain(body, accessors, model, model.Because, "Because", ref counter);
        var destroy = hasError ? null : Chain(body, accessors, model, model.Destroy, "Destroy", ref counter);
        var facts = new List<string>();
        for (var index = 0; index < model.Facts.Count; index++)
        {
            facts.Add(Fact(body, accessors, model, model.Facts[index], $"Fact{index}", ref counter));
        }

        var create = hasError ? "null" : "Create";
        if (!hasError)
        {
            if (model.ConstructorIsAccessible)
            {
                body.Append("        static object Create() => new ").Append(model.FullyQualifiedName).AppendLine("();");
            }
            else
            {
                accessors.AppendLine("        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]");
                accessors.Append("        static extern ").Append(model.FullyQualifiedName).AppendLine(" Construct();");
                body.AppendLine("        static object Create() => Construct();");
            }
        }

        builder.Append(accessors).Append(body);
        builder.AppendLine("        internal static void Register() =>");
        builder.Append("            ").Append(Runtime).AppendLine(".SpecificationRegistry.Register(typeof(global::Cratis.Specifications.Generated.SpecificationRegistration).Assembly, new(");
        builder.Append("                ").Append(Literal(model.Uid)).AppendLine(",");
        builder.Append("                ").Append(Literal(model.Namespace)).AppendLine(",");
        builder.Append("                ").Append(Literal(model.TypeName)).AppendLine(",");
        builder.Append("                ").Append(Literal(model.DisplayName)).AppendLine(",");
        builder.Append("                ").Append(create).AppendLine(",");
        builder.Append("                ").Append(establish ?? "null").AppendLine(",");
        builder.Append("                ").Append(because ?? "null").AppendLine(",");
        builder.Append("                ").Append(destroy ?? "null").AppendLine(",");
        builder.Append("                new ").Append(Runtime).AppendLine(".FactDescriptor[]");
        builder.AppendLine("                {");
        foreach (var fact in facts) builder.Append("                    ").Append(fact).AppendLine(",");
        builder.AppendLine("                },");
        builder.Append("                ").Append(model.Error is null ? "null" : Literal(model.Error)).AppendLine("));");
        builder.AppendLine("    }");
    }

    static string? Chain(StringBuilder body, StringBuilder accessors, SpecificationModel model, EquatableArray<MethodCall> calls, string name, ref int counter)
    {
        if (calls.Count == 0) return null;
        var isAsync = calls.Any(_ => _.Returns != ReturnKind.Void || _.How == CallKind.Reflection);
        body.Append("        static ").Append(isAsync ? "async " : string.Empty).Append(ValueTask).Append(' ').Append(name).AppendLine("(object instance)");
        body.AppendLine("        {");
        body.Append("            var specification = (").Append(model.FullyQualifiedName).AppendLine(")instance;");
        body.AppendLine("            try");
        body.AppendLine("            {");
        foreach (var call in calls)
        {
            var expression = Call(accessors, call, ref counter);
            var awaited = call.Returns != ReturnKind.Void || call.How == CallKind.Reflection;
            body.Append("                ").Append(awaited ? "await " : string.Empty).Append(expression).AppendLine(";");
        }

        if (!isAsync) body.AppendLine("                return default;");
        body.AppendLine("            }");
        body.AppendLine("            catch (global::System.OperationCanceledException exception)");
        body.AppendLine("            {");
        body.Append("                throw new global::Cratis.Specifications.SpecificationCancelledException(").Append(Literal(name)).AppendLine(", exception);");
        body.AppendLine("            }");
        body.AppendLine("        }");
        return name;
    }

    static string Fact(StringBuilder body, StringBuilder accessors, SpecificationModel model, FactModel fact, string name, ref int counter)
    {
        body.Append("        static ").Append(ValueTask).Append(' ').Append(name).AppendLine("(object instance)");
        body.AppendLine("        {");
        body.Append("            var specification = (").Append(model.FullyQualifiedName).AppendLine(")instance;");
        var expression = Call(accessors, fact.Call, ref counter);
        if (fact.Call.How == CallKind.Reflection || fact.Call.Returns == ReturnKind.ValueTask)
        {
            body.Append("            return ").Append(expression).AppendLine(";");
        }
        else if (fact.Call.Returns == ReturnKind.Task)
        {
            body.Append("            return new ").Append(ValueTask).Append('(').Append(expression).AppendLine(");");
        }
        else
        {
            body.Append("            ").Append(expression).AppendLine(";");
            body.AppendLine("            return default;");
        }

        body.AppendLine("        }");
        return $"new {Runtime}.FactDescriptor({Literal(fact.Call.Name)}, {Literal(fact.DisplayName)}, {(fact.FilePath is null ? "null" : Literal(fact.FilePath))}, {fact.Line}, {name}, {(fact.SkipReason is null ? "null" : Literal(fact.SkipReason))}, {Literal(ReturnTypeName(fact.Call.Returns))})";
    }

    static string Call(StringBuilder accessors, MethodCall call, ref int counter)
    {
        var index = counter++;
        switch (call.How)
        {
            case CallKind.Direct:
                return $"(({call.DeclaringType})specification).{call.Name}()";
            case CallKind.Accessor:
                accessors.Append("        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = ").Append(Literal(call.Name)).AppendLine(")]");
                accessors.Append("        static extern ").Append(ReturnType(call.Returns)).Append(" Accessor").Append(index).Append('(').Append(call.DeclaringType).AppendLine(" instance);");
                return $"Accessor{index}(specification)";
            default:
                accessors.Append("        static readonly global::System.Reflection.MethodInfo _method").Append(index).Append(" = ").Append(Runtime)
                    .Append(".ReflectionInvoker.Find(typeof(").Append(call.DeclaringType).Append("), ").Append(Literal(call.Name)).AppendLine(");");
                return $"{Runtime}.ReflectionInvoker.Invoke(_method{index}, specification)";
        }
    }

    static string ReturnTypeName(ReturnKind kind) => kind switch
    {
        ReturnKind.Task => "System.Threading.Tasks.Task",
        ReturnKind.ValueTask => "System.Threading.Tasks.ValueTask",
        _ => "System.Void"
    };

    static string ReturnType(ReturnKind kind) => kind switch
    {
        ReturnKind.Task => "global::System.Threading.Tasks.Task",
        ReturnKind.ValueTask => ValueTask,
        _ => "void"
    };

    static string Literal(string value) => SymbolDisplay.FormatLiteral(value, quote: true);
}
