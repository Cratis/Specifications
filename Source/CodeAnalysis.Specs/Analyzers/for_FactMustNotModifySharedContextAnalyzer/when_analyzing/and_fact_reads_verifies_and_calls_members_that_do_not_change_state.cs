// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_reads_verifies_and_calls_members_that_do_not_change_state : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    using NSubstitute;

    public interface IService
    {
        int Get();
        void Run();
    }

    public class Owner : System.IDisposable
    {
        public Owner Child { get; set; }
        public int Count { get; set; }
        public int Sum(int a, int b) => a + b;
        public void Dispose() { }
    }

    public class when_something : Cratis.Specifications.Specification
    {
        Owner _owner;
        IService _service;
        int[] _values;
        int _count;

        int Total() => _count + _values.Length;

        void Prepare(Owner owner)
        {
            var local = 0;
            local++;
            owner.Count = local;
            System.Action later = () => _count = 1;
        }

        static void Configure(Owner owner) => owner.Count = 1;

        [Xunit.Fact]
        void should_only_read()
        {
            var count = _owner.Child.Count + _values[0] + Total() + _owner.Sum(1, 2);
            var copy = new Owner { Count = count, Child = new Owner { Count = 1 } };
            copy.Count = 2;
            copy.Dispose();
            Prepare(copy);
            Configure(copy);
            _service.Received().Get();
            _service.Received().Run();
            var other = _service;
            IService local = null;
            local.Get().Returns(1);
            Received.InOrder(() => _owner.Dispose());
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SubstituteSource + InOrderSource);

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
