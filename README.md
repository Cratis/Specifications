# Cratis Specifications

**Specification by Example (BDD) for .NET — Given/When/Then specs with xUnit, NUnit and TUnit, in the style of [Machine.Specifications](https://github.com/machine/machine.specifications).**

[![Build](https://github.com/Cratis/Specifications/actions/workflows/build.yml/badge.svg)](https://github.com/Cratis/Specifications/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/cratis.specifications.xunit?label=Cratis.Specifications.XUnit&logo=nuget)](https://www.nuget.org/packages/cratis.specifications.xunit)
[![Nuget](https://img.shields.io/nuget/v/cratis.specifications.xunit.v3?label=Cratis.Specifications.XUnit.v3&logo=nuget)](https://www.nuget.org/packages/cratis.specifications.xunit.v3)
[![Nuget](https://img.shields.io/nuget/v/cratis.specifications.nunit?label=Cratis.Specifications.NUnit&logo=nuget)](https://www.nuget.org/packages/cratis.specifications.nunit)
[![Nuget](https://img.shields.io/nuget/v/cratis.specifications.tunit?label=Cratis.Specifications.TUnit&logo=nuget)](https://www.nuget.org/packages/cratis.specifications.tunit)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Discord](https://img.shields.io/discord/1182595891576717413?label=Discord&logo=discord&logoColor=white)](https://discord.gg/kt4AMpV8WV)

Cratis Specifications brings Specification by Example — behavior-driven development (BDD) with
**given**, **when**, **then** — to .NET test projects, keeping the concise structure and syntax
popularized by Machine.Specifications while running on standard test frameworks. That means full
tooling support across IDEs, editors, and CI, with your choice of xUnit, NUnit or TUnit as the runner.
The background and motivation are described in
[this article](https://web.archive.org/web/20210922202940/https://www.ingebrigtsen.info/2021/09/05/specifications-in-xunit/).

Packages:

- [Cratis.Specifications.XUnit](https://www.nuget.org/packages/cratis.specifications.xunit) — for xUnit v2
- [Cratis.Specifications.XUnit.v3](https://www.nuget.org/packages/cratis.specifications.xunit.v3) — for xUnit v3
- [Cratis.Specifications.NUnit](https://www.nuget.org/packages/cratis.specifications.nunit) — for NUnit 4
- [Cratis.Specifications.NUnit5](https://www.nuget.org/packages/cratis.specifications.nunit5) — for NUnit 5
- [Cratis.Specifications.TUnit](https://www.nuget.org/packages/cratis.specifications.tunit) — for TUnit
- [Cratis.Specifications.Runner](https://www.nuget.org/packages/cratis.specifications.runner) — the native runner on Microsoft.Testing.Platform
- [Cratis.Specifications](https://www.nuget.org/packages/cratis.specifications) — the shared core (the `Specification` base, assertions and analyzers); every package above depends on it

All packages target `net10.0`; specification projects that must target .NET 8 or .NET 9 stay on 4.x. Use the same version of
`Cratis.Specifications` and of your framework package. Coming from 4.x? Read the [5.0 upgrade guide](./Documentation/upgrading-5.0.md).

## What does it do?

In BDD one talks about the **given**, **when**, **then**. Much like **arrange**, **act** and **assert** in a way that
is more common in TDD. The biggest difference is on a mindset level of thinking in specifications of behaviors in your
system. What this particular library delivers is a way to do these and also keep in line with what is common in the BDD
world of having isolated specifications and not have typically a **FooTests** and dump all your tests for the unit `Foo` in
it.

The library supports the convention lifecycle methods `Establish()` (or `Given()`), `Because()` (or `When()`) and `Destroy()`. There is no virtual method
to override, just match the expected signatures:

| Signature | Purpose |
| --------- | ------- |
| void Establish() | Establishes the current context - **given** / **arrange** |
| void Because() | Triggers the behavior being specified - **when** / **act** |
| void Destroy() | Tears down the context |

If your specification requires to run in an async context, it also supports the following:

| Signature | Purpose |
| --------- | ------- |
| Task Establish() | Establishes the current context - **given** / **arrange** |
| Task Because() | Triggers the behavior being specified - **when** / **act** |
| Task Destroy() | Tears down the context |

All lifecycle methods are optional and will be ignored if not there. `ValueTask` is also supported.
Aliases have the same signatures and execution order as their conventional names. You can mix
names across base and derived contexts, but declaring both names for one phase on the same type
throws `AmbiguousLifecycleMethod` and reports analyzer error `CRSPEC0009`.
Multiple levels of inheritance recursively is supported, meaning that specifications will run all the lifecycle methods
from the lowest level in the hierarchy chain and up the hierarchy (e.g. no_user_authenticated -> when_authenticating_a_null_user).

To get all this to work, all you need to do is inherit from the `Specification` type found in `Cratis.Specifications`.

## Structure and naming

The general purpose of BDD and specification by example is to make it all very human readable and possible to navigate quite
easily. New developers can come into the solution and pretty much read up on the specifications and get a glimpse of how the
system works. So rather than having a **FooTests** class with all the tests, it is recommended to have folders describing the scenario being
specified. For a unit this could be named `for_<name of unit>` e.g. : `for_SecurityService`. If you're testing a more domain
centric scenario in your system that involves multiple units, the folder name would reflect the name of the scenario e.g.:
`for_logging_in_users`.

Within these folders you'd keep your **when** statements. E.g. **When_authenticating_an_admin_user**. If you want to group things,
for instance lets say you have multiple behaviors within the concept of **authenticating**, you could then have a folder grouping these
called **When_authenticating** and then drop in the behavior specifications within this folder **an_admin_user** and **a_null_user**.

In addition to this you might want to reuse a context. This can quite easily be achieved through inheritance. Structure-wise you'd
then have a **given** folder and namespace where you'd put the common reusable context - again reflecting what it represents,
for instance for our authentication scenario: **no_user_authenticated**.

For a sample of how this looks like, look within the [sample](./Sample) folder.

## Setting up a project

Choose one specification package and its runner packages. In each case, use `Cratis.Specifications`
for the `Specification` base and assertions; keep the framework's own attribute (`[Fact]` or `[Test]`).

| Framework | Specification package | Runner packages | Project defaults supplied |
| --- | --- | --- | --- |
| xUnit v2 | `Cratis.Specifications.XUnit` | `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio` | Shared lifecycle and readable names on; the framework registration is generated |
| xUnit v3 | `Cratis.Specifications.XUnit.v3` | `xunit.v3.mtp-off`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio` (or the MTP-enabled `xunit.v3` runner) | Same as xUnit v2; set `<OutputType>Exe</OutputType>` yourself |
| NUnit 4 | `Cratis.Specifications.NUnit` | `Microsoft.NET.Test.Sdk`, `NUnit3TestAdapter` | Readable names on |
| NUnit 5 | `Cratis.Specifications.NUnit5` | `Microsoft.NET.Test.Sdk`, `NUnit3TestAdapter` | Readable names on |
| TUnit | `Cratis.Specifications.TUnit` | `TUnit` | Readable names on; the `TUnit` package makes it an executable |
| Native runner | `Cratis.Specifications.Runner` | None | Executable Microsoft.Testing.Platform test project with generated entry point; readable names |

The same optional properties work for every package. The values below turn a default **off**:

```xml
<PropertyGroup>
    <CratisSpecificationsSharedLifecycle>false</CratisSpecificationsSharedLifecycle>
    <CratisSpecificationsReadableNames>false</CratisSpecificationsReadableNames>
    <CratisSpecificationsAnalyzers>false</CratisSpecificationsAnalyzers>
</PropertyGroup>
```

| Property | Default | What it sets |
| --- | --- | --- |
| `CratisSpecificationsSharedLifecycle` | `true` | xUnit: generates the framework registration attribute, sharing eligible specification instances. Set to `false` to keep xUnit's per-fact lifecycle (the build emits `[assembly: PerFactSpecificationLifecycle]`). NUnit, TUnit and the native runner already share instances; this property is accepted without changing their lifecycle. |
| `CratisSpecificationsReadableNames` | `true` | Generates `[assembly: ReadableSpecificationNames]`; set to `false` to keep the original display names, in every package including the native runner. xUnit also registers the framework; if shared lifecycle is turned off it emits `[assembly: PerFactSpecificationLifecycle]` to retain per-fact execution. TUnit additionally generates its display-name formatter attribute. |
| `CratisSpecificationsAnalyzers` | `true` | `false` turns the bundled analyzers off, `ide` keeps them in the IDE only. See [Turning the analyzers off](#turning-the-analyzers-off). |
| `CratisSpecificationsSharedResources` | `false` | Experimental; set to `true` to lease isolated parts of [shared resources](./Documentation/shared-resources.md). |
| `CratisSpecificationsPhaseTimings` | `false` | Set to `true` to record [phase timings](./Documentation/phase-timings.md); `CratisSpecificationsSlowPhaseThreshold` (milliseconds) and `CratisSpecificationsPhaseTimingsFile` refine it. |

xUnit projects share the specification lifecycle by default: `Establish` and `Because` run once per class. If your xUnit project already
has a handwritten `[assembly: Xunit.TestFramework(...)]`, **remove it**; duplicate framework attributes cause `CS0579`.
To keep per-fact execution for one class or the whole assembly, apply `[PerFactSpecificationLifecycle]` to the class or assembly, or set
`CratisSpecificationsSharedLifecycle` to `false`. `CRSPEC0008` (an error by default) is configured through `.editorconfig` only; see [Analyzers](#analyzers).
Registering `SpecificationTestFramework` by hand continues to mean shared lifecycle. For a
names-only manual registration, also add `[assembly: PerFactSpecificationLifecycle]` and
`[assembly: ReadableSpecificationNames]`.

Readable names use the same formatter in every package: for example,
`for_Author.when_registering.should_keep_the_name` becomes
`Author › when registering › should keep the name`. Namespace prefixes before `for_` are
omitted and nested specification type names are retained. Display names change, not test
identities, namespace filters, traits or skip conditions. Explicit xUnit fact display names
remain unchanged. `SpecificationDisplayNames.ForType`, `ForClass`, `ForFact` and `Readable`
expose the formatter for integrations (hidden from IntelliSense).

## Compiler Warnings

Since the naming of classes, methods and structure deviates from what is expected by default from the C# compiler, you typically
end up getting a lot of warnings. These can be turned off by adding a **NoWarn** element within a **PropertyGroup** to your `.csproj` file:

```xml
<PropertyGroup>
    <NoWarn>CA1707;CS1591;RCS1213;IDE0051;IDE1006;CA1051</NoWarn>
</PropertyGroup>
```

| Warning | Description |
| ------- | ----------- |
| [CA1707](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1707) | Identifiers should not contain underscores |
| [CA1051](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/CA1051) | Do not declare visible instance fields |
| [CS1591](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591)  | Missing XML comment for publicly visible type or member 'Type_or_Member' |
| [IDE0051](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0051) | Remove unused private member |
| [IDE1006](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/naming-rules#rule-id-ide1006-naming-rule-violation) | Naming rule violation |
| [RCS1213](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1213/) | Remove unused member declaration|

If you're using static code analysis and stylecop and have turned on all rules by default, you might also encounter the following that you want to turn off:

| Warning | Description |
| ------- | ----------- |
| [SA1633](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1633.md) | File header copyright text must match |
| [SA1649](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1649.md) | File name must match type name |
| [SA1600](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1600.md) | Elements must be documented |
| [SA1310](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1310.md) | Field names must not contain underscore |
| [SA1502](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1502.md) | Element must not be on a single line |
| [SA1134](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1134.md) ||

Depending on your solution, you might want to consider suppressing the following.

| Warning | Description |
| ------- | ----------- |
| [RCS1090](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1090/) | Add call to 'ConfigureAwait'.|

## Example

In the [sample](./Sample) folder you'll find samples of using it XUnit, NUnit and TUnit.

### NUnit 4 or NUnit 5

Pick the package that matches the NUnit version your test project uses. Both offer the same API and
the same namespace, so switching is only a package change:

| NUnit version | Package |
| ------------- | ------- |
| 4.x | `Cratis.Specifications.NUnit` |
| 5.x | `Cratis.Specifications.NUnit5` |

The assertions behave the same in both; see [Assertions](#assertions).

The difference between XUnit and NUnit is on the signature of the tests.
Instead of `[Fact]` for XUnit, you'll have to adorn with `[Test]`, in addition to that, NUnit
requires methods to be public to run them. TUnit uses `[Test]` as well, and also requires public methods.

Basically, building on the Machine.Specifications sample - this would become:

```csharp
class When_authenticating_an_admin_user : Specification
{
    SecurityService subject;
    UserToken user_token;

    void Establish() =>
             subject = new SecurityService();

    void Because() =>
             user_token = subject.Authenticate("username", "password");

    [Fact] void should_indicate_the_users_role() =>
        user_token.Role.ShouldEqual(Roles.Admin);

    [Fact] void should_have_a_unique_session_id() =>
        user_token.SessionId.ShouldNotBeNull();
}
```

Catching an exception and testing for the correct exception:

```csharp
class When_authenticating_a_null_user : Specification
{
    SecurityService subject;
    Exception result;

    void Establish() =>
             subject = new SecurityService();

    void Because() =>
             result = Catch.Exception(() => subject.Authenticate(null, null));

    [Fact] void should_throw_user_must_be_specified_exception() =>
        result.ShouldBeOfExactType<UserMustBeSpecified>();
}
```

Building reusable contexts (in a sub-namespace with given):

```csharp
class no_user_authenticated
{
    protected SecurityService subject;

    void Establish() =>
             subject = new SecurityService();
}
```

Refactor one of the specifications:

```csharp
class When_authenticating_a_null_user : given.no_user_authenticated
{
    Exception result;

    void Because() =>
             result = Catch.Exception(() => subject.Authenticate(null, null));

    [Fact] void should_throw_user_must_be_specified_exception() =>
        result.ShouldBeOfExactType<UserMustBeSpecified>();
}
```

Supports teardown through `destroy`:

```csharp
class no_user_authenticated
{
    protected SecurityService subject;

    void Establish() =>
             subject = new SecurityService();

    void Destroy() => subject.Dispose();

}
```

## Using TUnit

Add the `Cratis.Specifications.TUnit` package next to the [`TUnit`](https://tunit.dev) package in your test project:

```bash
dotnet add package TUnit
dotnet add package Cratis.Specifications.TUnit
```

Specifications derive from `Specification` exactly as with the other frameworks, and facts are public `[Test]` methods:

```csharp
public class when_adding_an_item : Specification
{
    List<string> _items;

    void Establish() => _items = [];
    void Because() => _items.Add("item");

    [Test] public void should_contain_the_item() => _items.ShouldContain("item");
}
```

The lifecycle matches the xUnit shared mode and NUnit: each specification class is constructed **once**,
`Establish` and `Because` run **once** before the first fact, all facts share that instance, and `Destroy`
runs **once** after the last fact. If `Establish` or `Because` fails, every fact of the class fails.
TUnit may run the facts of a class in parallel, so facts must only read from the shared context. Because the
instance is shared, do not implement `IDisposable` on a specification: TUnit would dispose it after every
fact. Use `Destroy` instead.

TUnit assertions are asynchronous, so the `Should*` extension methods in this package are implemented
without them and throw a `SpecificationAssertionException` with the expected and actual values. They have the
same names and signatures as in the xUnit package.

TUnit runs on [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro).
On the .NET 10 SDK, `dotnet test` for a TUnit project requires opting in to the Microsoft.Testing.Platform runner in
`global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`), which applies to every test project in the
repository. Use `dotnet run --project <spec project>` when the repository also has xUnit or NUnit projects on VSTest,
as this repository does.

## Shared instance per specification class

xUnit constructs a fresh instance of the test class for every `[Fact]`, which would make `Establish` and `Because` run once
**per fact** rather than once per behavior. For specs whose `Because` drives real machinery, that multiplies the cost of the
action by the number of facts, and a behavior described by several facts ends up being executed several times.

The xUnit packages therefore share the instance by default, with MSpec-style semantics: every eligible
specification class is constructed **once**, `Establish` and `Because` run **once**, all its facts run against that shared
instance, and `Destroy` runs **once** after the last fact. NUnit, TUnit and the native runner work the same way. The
package build generates the framework registration; you write nothing. A hand-written
`[assembly: Xunit.TestFramework(...)]` for Cratis Specifications must be removed, otherwise the build fails with `CS0579`.

A class is eligible when it derives from `Specification`, has a single public constructor
with no arguments or only framework-managed fixture arguments, and contains only plain facts.
Class and collection fixtures stay owned by xUnit. `IDisposable` and `IAsyncDisposable`
specifications are supported: `Destroy` runs once, then disposal runs once even if `Destroy`
fails. Async disposal takes precedence when both disposal interfaces are implemented.
Plain xUnit classes, theories, custom `IAsyncLifetime` implementations, `ITestOutputHelper`
constructors and unsupported constructor arguments keep the stock per-fact lifecycle.
The same rules apply on v3, including its assembly fixtures.

The one rule to honor: facts must not mutate the shared context. They are assertions about the
single execution the class describes, which is the BDD model to begin with. The `CRSPEC0008` analyzer enforces it as a build error,
and `CRSPEC0012` warns about calls that probably change it, such as executing a command in a fact.

To keep xUnit's per-fact lifecycle, apply `[PerFactSpecificationLifecycle]` to a class or to the assembly, or set
`<CratisSpecificationsSharedLifecycle>false</CratisSpecificationsSharedLifecycle>` for the whole project.

## xUnit v2 and xUnit v3

Pick the package that matches the xUnit version of your test project:

| xUnit version | Package | Test framework packages |
| --- | --- | --- |
| v2 (`xunit` 2.x) | `Cratis.Specifications.XUnit` | `xunit`, `xunit.runner.visualstudio` |
| v3 (`xunit.v3` 4.x) | `Cratis.Specifications.XUnit.v3` | `xunit.v3` (or `xunit.v3.mtp-off`), `xunit.runner.visualstudio` |

Both packages expose the same `Specification` base class, `Establish`/`Because`/`Destroy` conventions
and `Should*` assertion extensions, so specifications move between them unchanged. An xUnit v3 test
project is an executable, so set `<OutputType>Exe</OutputType>` as described in the
[xUnit v3 migration guide](https://xunit.net/docs/getting-started/v3/migration).

The shared instance per specification class is the default on xUnit v3 too, with the same eligibility rules and the
same opt-outs.

## Assertions

The `Should*` assertions live in `Cratis.Specifications` and behave the same in every package: the same names, rules and
failure messages. Only the exception type differs, because each package fails the way its test framework expects:

| Package | Exception thrown by a failed assertion |
| --- | --- |
| xUnit v2 and v3 | `XunitException` |
| NUnit 4 and 5 | NUnit's `AssertionException` |
| TUnit and the native runner | `SpecificationAssertionException` (carries `Expected` and `Actual` for a comparison) |

| Behavior | Rule in every package |
| --- | --- |
| `ShouldEqual(IEnumerable)` and `ShouldEqual(params)` | Same elements in the same order. |
| `ShouldEqualInOrder(...)` | Same as `ShouldEqual` for collections, by name. |
| `ShouldEqualInAnyOrder(...)` | Same elements in any order; duplicates are significant. |
| `ShouldContainOnly` | Any order; duplicates count. |
| `ShouldContain(items)` | Every item is present; repeats are not demanded. |
| `ShouldContainSingleMatching(predicate)` | Exactly one element matches. |
| Nested collection elements | Compared deeply, element by element (strings are compared as strings). |
| Actual is `null` | Fails with a readable message instead of throwing a `NullReferenceException`. |
| Expected is `null` | Equal to a `null` actual; a `null` expected collection fails. |
| Predicates (`ShouldMatch`, `ShouldEachConformTo`, containment by predicate) | Take `Func<T, bool>`. |

A failed `null` check reads like `ShouldContain expected the actual value to contain the expected item but the actual value was null`.
`ShouldNotBeNull` is annotated `[NotNull]`, so the compiler knows the value is not null afterwards.

In NUnit, a failing `Should*` assertion inside `Assert.Multiple` stops the block at that assertion; later assertions in the
block do not report. Moving from 4.x? See the [5.0 upgrade guide](./Documentation/upgrading-5.0.md#assertions).

## Analyzers

`Cratis.Specifications` ships Roslyn analyzers (`CRSPEC0001`–`CRSPEC0009` and `CRSPEC0012`) that catch common specification mistakes; the
framework packages receive them through their dependency on it. The native runner's source generator adds `CRSPEC0010`
(an `async void` fact or lifecycle method, an error) and `CRSPEC0011` (a method called through reflection, a warning).

Two rules protect the shared specification context. Facts share one context by default, so a fact that changes it
makes the other facts depend on the order they run in. Both tell you to move the change into `Because` (or
`Establish`), or to mark the class `[PerFactSpecificationLifecycle]` so each fact gets its own context.

`CRSPEC0008` — *facts should not modify the shared specification context* — is an **error**. It reports changes the
analyzer can prove from the code. In a `should_*` fact, it reports:

- assigning to, or incrementing, an instance field or property of the specification, or anything reached through one:
  `_value = 1`, `_owner.Count = 2`, `_counts["a"] = 3`, `_values[0]++`, `_owner.Changed += handler`;
- calling `Add`/`AddRange`/`Remove`/`RemoveAt`/`Clear`/`Insert` on such a field, when the result is discarded;
- disposing such a field, or an object reached through one (`IDisposable.Dispose`, `IAsyncDisposable.DisposeAsync`);
- reconfiguring an NSubstitute substitute held in such a field: `Returns`, `Throws`, `When`, `ClearReceivedCalls`
  and their variants, as in `_service.Get().Returns(1)`;
- calling a method of the specification that does any of the above, when that method is declared in the same file.
  The message names the method: `modifies the shared specification state '_count' through 'Reset()'`.

`CRSPEC0012` — *facts should not call members that change the shared specification context* — is a **warning**,
because it judges a call by the member's name rather than by what the member does. It reports calls on a field or
property of the specification, or an object reached through one, to:

- members that run the action under test: `Execute`, `ExecuteAsync`, `Handle`, `HandleAsync`;
- consuming reads: `Take`, `TryTake`, `Dequeue`, `TryDequeue`, `Pop`, `TryPop`, `MoveNext`;
- other changes: `Push`, `Enqueue`, `TryAdd`, `TryRemove`, `TryUpdate`, `AddOrUpdate`, `RemoveAll`, `Reset`;
- `Add`/`AddRange`/`Remove`/`RemoveAt`/`Clear`/`Insert` on an object reached through a field, such as
  `_owner.Items.Clear()`, when the result is discarded.

Both rules see through null-conditional access, so `_items?.Clear()`, `_owner?.Count = 1` and `_store?.Dequeue()`
are reported like their plain forms. Neither rule reports in classes or assemblies marked
`[PerFactSpecificationLifecycle]` or in theories. Neither reports calls verified through `Received()`,
`DidNotReceive()` or inside `Received.InOrder`, calls on immutable collections, LINQ operators such as `Take`, or
reads. `CRSPEC0012` does not report a call whose result is handed to NSubstitute, as in `_scenario.Execute(1).Returns(x)`;
`CRSPEC0008` reports that as reconfiguring the substitute. Changes made through a method declared in another file
(a `given` context), through lambdas inside a helper, and inside iterator helpers (methods using `yield`, whose body
runs only when enumerated) are not followed. Lower either rule for a project or folder in an `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.CRSPEC0008.severity = warning   # or suggestion, or none
dotnet_diagnostic.CRSPEC0012.severity = none
```

The `CratisSpecificationsWarnOnSharedContextMutation` property from 4.x is gone.

### Optional runtime tuning and NUnit parallelism

`CratisSpecificationsRuntimeDefaults=true` opts into non-concurrent GC and disables dynamic PGO;
explicit `ConcurrentGarbageCollection` and `TieredPGO` properties always win. `false` leaves SDK settings unchanged.
`CratisSpecificationsParallelFixtures=true` opts NUnit 4/5 into fixture parallelism; unset or `false`
emits no attribute. Both switches are off by default. Do not combine the NUnit switch with an existing
assembly-level `Parallelizable` attribute. See [Running specifications fast](./Documentation/running-fast.md)
for trade-offs and how to run a built executable after restoring and building once.

### Turning the analyzers off

The analyzers cost build time in large repositories. Turn them off with one property; it removes only the
`Cratis.Specifications.CodeAnalysis` analyzers, so other analyzers and the native runner's source generator keep working:

```xml
<PropertyGroup>
    <CratisSpecificationsAnalyzers>false</CratisSpecificationsAnalyzers>
</PropertyGroup>
```

| Value | Effect |
| --- | --- |
| Unset or `true` | The analyzers run (default). |
| `false` | The analyzers never run, in command-line builds or in the IDE. |
| `ide` | The analyzers run in IDE design-time builds only (Visual Studio, Rider and VS Code design-time builds count as IDE), not in command-line or CI builds. |

Standard SDK switches also work:

- `RunAnalyzersDuringBuild=false` or `RunAnalyzers=false` turns off **all** analyzers, not only ours.
- Set a single rule's severity in `.editorconfig`: `dotnet_diagnostic.CRSPEC0008.severity = none`.
- Do not use `ExcludeAssets="analyzers"` to drop them. The analyzers come from the `Cratis.Specifications` dependency, so
  excluding assets on a framework package does not remove them, and on `Cratis.Specifications.Runner` it also drops the
  source generator, so the specifications are no longer registered. Use the property.

## Native runner on Microsoft.Testing.Platform

`Cratis.Specifications.Runner` is a native specification runner on
[Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro),
without xUnit, NUnit or TUnit underneath. Reference the package and the test project becomes an executable test
application; a source generator registers the specifications at compile time. Ordinary methods use generated
invocation; inaccessible methods on constructed generic base types use a reflection fallback.

```csharp
public class when_registering : Specification
{
    Author _author;

    void Given() => _author = new Author();            // or Establish
    void When() => _author.Register("Jane");          // or Because

    void should_be_registered() => _author.IsRegistered.ShouldBeTrue();
}
```

- Facts are methods named `should_*`; no attribute is needed (`[Fact]` is accepted too, and `[Fact(Skip = "...")]` skips).
- `Given`/`When` are aliases for `Establish`/`Because`; a class may not declare both forms of the same step.
- One instance per class: `Establish` and `Because` run once, facts may run concurrently and must only read the
  shared context, then `Destroy` and disposal (`IDisposable`/`IAsyncDisposable`) run once.
- Stop/cancellation stops scheduling new classes, then waits for running classes to finish all their facts, `Destroy`
  and disposal. Unstarted facts are reported as skipped, except after a hard IDE stop, where the host suppresses further updates (see [Limits](./Documentation/runner.md#limits)). Running code can cooperate through
  `SpecificationContext.CancellationToken`, which flows across awaits and is isolated per class; outside a class it
  is `CancellationToken.None`. Cancellation does not forcibly interrupt running code.
- `--treenode-filter '/<assembly>/<namespace>/<class>/<fact>'` selects methods by their code names, not their
  readable display names. Segments support the platform's wildcards and alternatives; for example
  `--treenode-filter '/*/*/when_registering/*'`. Use the assembly's simple name and URL-encode special characters
  in path segments. There are no trait properties. `--filter-uid` still accepts class or fact UIDs; other filter
  types fail explicitly rather than running an unfiltered suite.
- Test names read as sentences: `for_Author.when_registering.should_be_registered` shows as
  `Author › when registering › should be registered`.
- Options: `--specs-parallel <n>`, `--specs-sequential-facts`, `--fail-fast`, `--last-failed-first`,
  `--shard <index>/<count>` and `--specs-cache <path|none>`; standard options such as `--filter-uid` and `--report-trx` apply.

Run it with `dotnet run --project <spec project>` or the built executable. The package, its options and its
behavior follow the same versioning as the rest of the packages.

## Guides

- [Upgrading to 5.0](./Documentation/upgrading-5.0.md) — what changes from 4.x and a checklist.
- [The native runner](./Documentation/runner.md) — IDE support, options, limits and migrating from xUnit.
- [Runner output and reports](./Documentation/runner-output.md) — the console tree and Markdown, JSON and GitHub reports.
- [Phase timings](./Documentation/phase-timings.md) — where the time of a run goes, and slow-phase reporting.
- [Shared resources](./Documentation/shared-resources.md) (experimental, opt in with `CratisSpecificationsSharedResources=true`) — share a database or container across specifications and lease isolated parts of it.
- [Running specifications fast](./Documentation/running-fast.md) — package and runner choice, shared lifecycle, filtering, sharding and a short feedback loop.
- [Structuring specifications](./Documentation/structuring-specifications.md) — from a single behavior to shared contexts, doubles, async work and readable names.
- [Integration specifications](./Documentation/integration-specifications.md) — in-process scenarios, shared fixtures and keeping infrastructure-backed specifications fast.

## Related projects

Specifications pairs naturally with [Synopsis](https://github.com/Cratis/Synopsis), which turns
your specs into living documentation, and it is the testing style used throughout the Cratis
stack — including [Chronicle](https://github.com/Cratis/Chronicle), the event-sourcing database
and runtime, and [Arc](https://github.com/Cratis/Arc), the CQRS application framework for
ASP.NET Core.

## The Cratis ecosystem

This project is part of [Cratis](https://www.cratis.io) — free, MIT-licensed tools for building event-sourced and CQRS applications.

- **[Chronicle](https://github.com/Cratis/Chronicle)** — event-sourcing database and runtime. Orleans-based kernel, pluggable storage (MongoDB default; PostgreSQL, SQL Server, SQLite, in-memory), language-agnostic gRPC contracts. [Docs](https://www.cratis.io/chronicle/)
- **Chronicle clients** — first-class [.NET SDK](https://github.com/Cratis/Chronicle), plus [TypeScript](https://github.com/Cratis/Chronicle.TypeScript), [Kotlin/Java](https://github.com/Cratis/Chronicle.Kotlin), and [Elixir](https://github.com/Cratis/Chronicle.Elixir); [Python](https://github.com/Cratis/Chronicle.Python) coming soon (pre-alpha). AI agents connect through the [Chronicle MCP server](https://github.com/Cratis/Chronicle.Mcp).
- **[Arc](https://github.com/Cratis/Arc)** — opinionated CQRS framework for ASP.NET Core with commands, queries, validation, authorization, and TypeScript proxy generation. Works without event sourcing. [Docs](https://www.cratis.io/arc/)
- **[Components](https://github.com/Cratis/Components)** — React components aligned with Arc patterns. [Docs](https://www.cratis.io/components/)
- **[CLI](https://github.com/Cratis/cli) + Workbench** — inspect and diagnose Chronicle from the terminal or the browser. [Docs](https://www.cratis.io/cli/)
- **Model-first layer (experimental)** — Studio, [Screenplay](https://github.com/Cratis/Screenplay), [Stage](https://github.com/Cratis/Stage), [Scene](https://github.com/Cratis/Scene), [Prologue](https://github.com/Cratis/Prologue)
- **Supporting** — [Fundamentals](https://github.com/Cratis/Fundamentals), [Specifications](https://github.com/Cratis/Specifications) (this project), [Synopsis](https://github.com/Cratis/Synopsis), [Lens](https://github.com/Cratis/Lens), [Narrator](https://github.com/Cratis/Narrator), and free [AI tooling](https://github.com/Cratis/AI) (preview); Ensemble coming soon (pre-release)
- **[Samples](https://github.com/Cratis/Samples)** — runnable event sourcing and CQRS samples for the whole stack

Everything Cratis publishes today is MIT licensed and free to use.

Release notes and announcements: the [Cratis blog](https://blog.cratis.io).
