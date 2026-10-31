// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.when_analyzing;

public class and_fact_reads_verifies_or_uses_values_that_do_not_change : given.a_fact_should_not_call_state_changing_members_analyzer
{
    const string Usage = """
    using System.Linq;
    using NSubstitute;

    public class when_something : Cratis.Specifications.Specification
    {
        System.Collections.Generic.List<int> _items;
        System.Collections.Immutable.ImmutableStack<int> _stack;
        System.Collections.Immutable.ImmutableQueue<int> _queue;
        Scenario _scenario;
        Handler _handler;
        Owner _owner;

        [Xunit.Fact]
        void should_not_change()
        {
            var first = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Take(_items, 2));
            var taken = _items.Take(2);
            var popped = _stack.Pop();
            var dequeued = _queue.Dequeue();
            var added = _owner.Items.Count;
            var copy = new System.Collections.Generic.List<int>(_owner.Items);
            copy.Add(1);
            var store = new Store();
            store.Dequeue();
            _items.Add(1);
            _scenario.Received().Execute(1);
            Received.InOrder(() => _handler.Handle(1));
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SupportSource);

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
