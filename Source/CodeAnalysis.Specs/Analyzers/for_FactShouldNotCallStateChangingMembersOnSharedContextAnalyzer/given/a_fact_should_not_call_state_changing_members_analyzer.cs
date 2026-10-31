// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.given;

public class a_fact_should_not_call_state_changing_members_analyzer : Specification
{
    protected const string SupportSource = """
    namespace NSubstitute
    {
        public static class Received
        {
            public static void InOrder(System.Action action) { }
        }

        public static class SubstituteExtensions
        {
            public static T Received<T>(this T substitute) => substitute;
        }
    }

    namespace Sample
    {
        public class Scenario
        {
            public System.Threading.Tasks.Task Execute(int command) => System.Threading.Tasks.Task.CompletedTask;
        }

        public class Handler
        {
            public void Handle(int command) { }
        }

        public class Store
        {
            public int Take() => 0;
            public int Dequeue() => 0;
            public int Pop() => 0;
            public bool MoveNext() => false;
            public void Enqueue(int item) { }
            public bool TryAdd(int item) => true;
            public void Reset() { }
        }

        public class Owner
        {
            public System.Collections.Generic.List<int> Items { get; set; }
            public Store Store { get; set; }
        }
    }
    """;
}
