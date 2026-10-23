// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_naming_a_standard_case : Specification
{
    XunitTestCase _original;
    SpecificationTestCase _named;

    void Because()
    {
        var assembly = new TestAssembly(Reflector.Wrap(typeof(Probe).Assembly));
        var collection = new TestCollection(assembly, null, "Naming");
        var type = new TestClass(collection, Reflector.Wrap(typeof(Probe)));
        var method = new TestMethod(type, Reflector.Wrap(typeof(Probe).GetMethod(nameof(Probe.should_work))));
        _original = new XunitTestCase(new NullMessageSink(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, method);
        _named = new SpecificationTestCase(new NullMessageSink(), _original);
    }

    [Fact] void should_use_the_shared_formatter() => _named.DisplayName.ShouldEqual(SpecificationDisplayNames.ForFact(SpecificationDisplayNames.ForType(typeof(Probe)), "should_work"));
    [Fact] void should_preserve_the_original_identity() => _named.UniqueID.ShouldEqual(_original.UniqueID);
    [Fact] void should_preserve_skip_metadata() => _named.SkipReason.ShouldEqual("deliberate skip");
    [Fact] void should_not_rename_the_original_case() => _original.DisplayName.ShouldContain("should_work");

    sealed class Probe : Specification
    {
        [probe_fact] public void should_work() { }
    }

    // This private probe is not discovered; supply skip metadata without skipping an executable specification.
    [AttributeUsage(AttributeTargets.Method)]
    sealed class probe_factAttribute : FactAttribute
    {
        public probe_factAttribute() => Skip = "deliberate skip";
    }
}
