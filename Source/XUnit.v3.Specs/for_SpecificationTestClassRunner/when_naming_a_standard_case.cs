// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.v3;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_naming_a_standard_case : Specification
{
    XunitTestCase _original;
    SpecificationTestCase _named;

    void Because()
    {
        var assembly = new XunitTestAssembly(typeof(Probe).Assembly, configFilePath: null);
        var collection = new XunitTestCollection(assembly, null, false, "Naming");
        var type = new XunitTestClass(typeof(Probe), collection);
        var method = new XunitTestMethod(type, typeof(Probe).GetMethod(nameof(Probe.should_work)), []);
        _original = new XunitTestCase(method, "Probe.should_work", "original-identity", false, skipReason: "deliberate skip");
        _named = new SpecificationTestCase(_original);
    }

    [Fact] void should_use_the_shared_formatter() => _named.TestCaseDisplayName.ShouldEqual(SpecificationDisplayNames.ForFact(SpecificationDisplayNames.ForType(typeof(Probe)), "should_work"));
    [Fact] void should_preserve_the_original_identity() => _named.UniqueID.ShouldEqual(_original.UniqueID);
    [Fact] void should_preserve_skip_metadata() => _named.SkipReason.ShouldEqual("deliberate skip");
    [Fact] void should_not_rename_the_original_case() => _original.TestCaseDisplayName.ShouldEqual("Probe.should_work");

    sealed class Probe : Specification
    {
        [Fact] public void should_work() { }
    }
}
