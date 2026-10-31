// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.given;

public class a_fact_must_not_modify_shared_context_analyzer : Specification
{
    protected const string InOrderSource = """
    namespace NSubstitute
    {
        public static class Received
        {
            public static void InOrder(System.Action action) { }
            public static System.Threading.Tasks.Task InOrder(System.Func<System.Threading.Tasks.Task> action) => action();
        }
    }
    """;

    protected const string SubstituteSource = """
    namespace NSubstitute
    {
        public class WhenCalled<T>
        {
            public void Do(System.Action<object> callback) { }
        }

        public static class SubstituteExtensions
        {
            public static T Returns<T>(this T value, T returnThis) => value;
            public static WhenCalled<T> When<T>(this T substitute, System.Action<T> call) => new();
            public static void ClearReceivedCalls<T>(this T substitute) { }
            public static T Received<T>(this T substitute) => substitute;
        }
    }
    """;
}
