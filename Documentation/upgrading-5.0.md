# Upgrading to Cratis Specifications 5.0

This guide lists what changes for a project that moves from Cratis Specifications 4.x to 5.0, and what to do about
each change. Most projects need three things: target .NET 10, remove a hand-written xUnit framework attribute, and fix
the facts that `CRSPEC0008` now reports as errors.

## Checklist

1. Target `net10.0` in every specification project. Projects that must stay on .NET 8 or .NET 9 stay on 4.x.
2. Move `Cratis.Specifications` and your framework package (`.XUnit`, `.XUnit.v3`, `.NUnit`, `.NUnit5`, `.TUnit` or
   `.Runner`) to the same 5.0 version.
3. xUnit: delete any `[assembly: Xunit.TestFramework(...)]` that registers Cratis Specifications.
4. xUnit: fix the facts that change the shared context (`CRSPEC0008`), or opt the class or assembly out of the shared
   lifecycle.
5. Native runner: make `async void` facts and lifecycle methods return `Task` or `ValueTask` (`CRSPEC0010`).
6. Change variables typed as `Predicate<T>` that you pass to assertions to `Func<T, bool>`.
7. NUnit 4: replace `ShouldEqual(params)` calls that relied on any order with `ShouldEqualInAnyOrder`.
8. Check `ShouldNotContain` calls on nullable strings: a `null` string fails in every package (4.3 and earlier let it pass in some). Use
   `(value ?? string.Empty).ShouldNotContain("x")` if `null` should count as empty.
9. Rebuild. Fix new nullable warnings if your project treats warnings as errors.
10. Expect one reset of test history in tools that key results by display name, and, for the native runner, by test
    identity (see [Readable names](#readable-names-are-on-by-default) and
    [Native runner](#native-runner)).

## Target framework

All packages target `net10.0` only. The 4.x packages also targeted .NET 8 and .NET 9. Specification projects that
still target those versions stay on 4.x; the libraries you test can keep targeting older versions, because only the
specification project references the package.

If a shared `Directory.Build.props` sets several target frameworks, override them in the specification project with the
plural property, so it replaces the inherited list instead of adding to it:

```xml
<PropertyGroup>
    <TargetFrameworks>net10.0</TargetFrameworks>
</PropertyGroup>
```

## xUnit shares the specification lifecycle by default

On xUnit v2 (`Cratis.Specifications.XUnit`) and xUnit v3 (`Cratis.Specifications.XUnit.v3`), `Establish` and `Because`
now run **once per specification class**, and every fact runs against that one instance. In 4.x this needed an opt-in.
NUnit, TUnit and the native runner already worked this way. The package registers the test framework for you; nothing
changes in the specification code.

Which classes are eligible is unchanged: the class derives from `Specification`, has a single public constructor
with no arguments (or only framework-managed fixture arguments) and contains plain facts. Theories, `ITestOutputHelper`
constructors, custom `IAsyncLifetime` implementations and unsupported constructor arguments keep xUnit's per-fact
lifecycle. The eligibility rules are in the [README](../README.md#shared-instance-per-specification-class).

### Remove a hand-written framework attribute

If a project registers the framework itself, the build now fails with `CS0579` (duplicate attribute):

```csharp
// Delete this line.
[assembly: Xunit.TestFramework("Cratis.Specifications.SpecificationTestFramework", "Cratis.Specifications.XUnit")]
```

### Keep per-fact execution

Facts that rely on a fresh instance for each fact must either be fixed or opt out. Pick the narrowest option:

| Scope | How |
| --- | --- |
| One class | `[PerFactSpecificationLifecycle]` on the class. |
| The whole assembly | `[assembly: PerFactSpecificationLifecycle]`. |
| The whole project | `<CratisSpecificationsSharedLifecycle>false</CratisSpecificationsSharedLifecycle>`; the build then emits `[assembly: PerFactSpecificationLifecycle]`. |

`PerFactSpecificationLifecycleAttribute` can now be applied to a class; in 4.x it was assembly-only. The attribute is in
the `Cratis.Specifications` namespace in both xUnit packages.

## `CRSPEC0008` is an error

`CRSPEC0008` (a fact modifies the shared specification context) was reported as info by default in 4.x. In 5.0 it is an
**error**, because facts now share one context by default and a fact that changes it makes the result depend on
the order the facts run in.

It reports a `should_*` fact that assigns to or increments an instance field or property of the specification, or calls
`Add`, `AddRange`, `Remove`, `RemoveAt`, `Clear` or `Insert` on one. It no longer reports in:

- classes or assemblies marked `[PerFactSpecificationLifecycle]`;
- theories;
- calls inside NSubstitute's `Received.InOrder` delegates, which record verification expectations;
- `Add`, `Remove` and `Clear` calls whose result is used or that target immutable collections.

Fix a reported fact by moving the change into `Establish` or `Because`:

```csharp
// Before: the fact changes the shared context, and CRSPEC0008 fails the build.
public class when_registering : Specification
{
    List<string> _names = [];

    void Because() => _names.Add("Jane");
    void should_have_one_name() { _names.Add("John"); _names.Count.ShouldEqual(2); }
}

// After: the fact only reads the context.
public class when_registering : Specification
{
    List<string> _names = [];

    void Because() { _names.Add("Jane"); _names.Add("John"); }
    void should_have_two_names() => _names.Count.ShouldEqual(2);
}
```

If the facts cannot share a context, mark the class with `[PerFactSpecificationLifecycle]`. To lower the severity for
a project or folder instead, use an `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.CRSPEC0008.severity = warning   # or suggestion, or none
```

The `CratisSpecificationsWarnOnSharedContextMutation` MSBuild property is **removed**. It raised the 4.x info to a
warning; setting it now has no effect. Delete it and use `.editorconfig` as shown above.

## Readable names are on by default

Every package now shows specifications as sentences: `for_Author.when_registering.should_keep_the_name` appears as
`Author › when registering › should keep the name`. The native runner already did.

- **What changes:** display names. Test ids, `--filter` expressions, namespace filters, traits and skip conditions are
  unchanged. Explicit xUnit fact display names are unchanged.
- **What to expect:** CI dashboards, reports and local history that key results by display name show the old names
  as removed and the new ones as added once.
- **To keep the original names**, set the property for any package:

```xml
<PropertyGroup>
    <CratisSpecificationsReadableNames>false</CratisSpecificationsReadableNames>
</PropertyGroup>
```

## Assertions

The `Should*` extension methods and `SpecificationAssertionException` now live in `Cratis.Specifications.dll`. Every
package forwards the moved types (`[TypeForwardedTo]`), so source and binaries that do not use a changed signature keep
working. Recompile your specification projects against 5.0.

All packages now apply the same rules and produce the same failure messages. Each package still fails with its own
exception type:

| Package | Exception thrown by a failed assertion |
| --- | --- |
| xUnit v2 and v3 | `XunitException` |
| NUnit 4 and 5 | NUnit's `AssertionException` |
| TUnit and the native runner | `SpecificationAssertionException`, which also carries `Expected` and `Actual` |

xUnit and NUnit assertion registration retains its XML documentation and uses an assembly-specific generated
namespace. Applications that combine documentation from several specification assemblies, including ASP.NET Core
OpenAPI applications, can load those XML files together without duplicate registration member IDs. No documentation
suppression or assertion-registration opt-out is needed; keep the core and framework packages on the same version.

What changes in behavior:

- **`ShouldEqual(params)` compares in order in every package.** NUnit 4 used to ignore the order. Use
  `ShouldEqualInAnyOrder(...)` where the order does not matter.
- **Nested collections are compared deeply.** Elements that are themselves collections (strings excluded) are compared
  recursively, also by `ShouldEqualInAnyOrder`, `ShouldContain` and `ShouldContainOnly`. Dictionaries compare as unordered
  key/value maps; other collections compare in order except sets, as described below. On xUnit this used to compare the
  nested collections by reference, so checks that failed for that reason now pass. A collection implementing
  `IEquatable<TSelf>` takes precedence over element comparison; an `Equals(object)` override alone does not. When the
  value is nested or typed as `object` or an interface, its `Equals(object)` must forward to its equatable equality.
  Implementations whose two equality overloads disagree can therefore differ from 4.x.
- **Sets compare without regard to enumeration order in every package.** Two `ISet<T>` or `IReadOnlySet<T>` values use
  unordered element comparison, including nested sets. This follows xUnit's set behavior; NUnit 4 previously compared
  sets in enumeration order. To check enumeration order instead, convert both sets to arrays before comparing them.
- **Native AOT test hosts** recognize dictionaries through the non-generic `IDictionary` interface, which the .NET
  dictionaries implement. A custom type that implements only the generic dictionary interfaces may compare in
  enumeration order there.
- **`null` is handled the same everywhere.** A `null` actual value fails with a readable message instead of throwing
  `NullReferenceException` or `ArgumentNullException`; a `null` expected value equals a `null` actual value; a `null`
  expected collection fails instead of throwing. **Breaking change if you upgrade from 4.3 or earlier:** `ShouldNotContain`
  on a `null` string fails in every package (since 4.4); in 4.3 and earlier some packages let it pass. This strict behavior is deliberate: a null subject
  is usually a bug, and the lenient behavior would hide it. If `null` should count as empty, coalesce it explicitly:

```csharp
(value ?? string.Empty).ShouldNotContain("x");
```

- **`Predicate<T>` is now `Func<T, bool>`** in the assertions that took a predicate (`ShouldMatch`,
  `ShouldEachConformTo`, `ShouldContain(filter)`, `ShouldNotContain(filter)` and `ShouldContainSingleMatching`).
  Lambdas compile unchanged. A variable or method group typed as `Predicate<T>` does not compile any more; change
  its type:

```csharp
// Before
Predicate<Author> isJane = _ => _.Name == "Jane";
// After
Func<Author, bool> isJane = _ => _.Name == "Jane";
```

- **NUnit `Assert.Multiple` stops at the first failing `Should*` assertion.** The NUnit 4 assertions used
  `Assert.That`, which records a failure inside `Assert.Multiple` and carries on. The assertions now throw
  `AssertionException` straight away, so the block ends at the first failing `Should*` call and later
  assertions in it do not report. Failures raised by NUnit's own `Assert.That` calls in the same block are
  still collected.

## Nullable annotations

The public API is annotated for nullable reference types: receivers are `object?`, `ShouldNotBeNull` carries
`[NotNull]` (so the compiler knows the value is not null afterwards), and `Catch` and the `Specification` members
carry nullable return types. Most projects see fewer warnings. If your build treats warnings as errors, a few new
ones are possible where a value was assumed to be non-null.

The NUnit 4 and NUnit 5 dictionary assertions `ShouldContain<TKey, TValue>(IDictionary<TKey, TValue>, TKey)` and
`ShouldNotContain<TKey, TValue>(IDictionary<TKey, TValue>, TKey)` now require `where TKey : notnull`, matching the
shared assertion implementation. Use non-nullable dictionary key types; generic helpers calling these assertions
must also constrain `TKey` with `where TKey : notnull`.

## Native runner

If you use `Cratis.Specifications.Runner`:

- **Test identity of async facts changes once.** The test method identifier now carries the fact's real return type
  (`Task`, `ValueTask` or `void`); 4.x always wrote `System.Void`. IDE and TRX history for `Task` and `ValueTask`
  facts therefore starts over once. Facts that return `void` keep their identity.
- **`CRSPEC0010` is an error.** An `async void` fact or lifecycle method cannot be awaited and loses its failures.
  Return `Task` or `ValueTask`. (The 4.x line reports this as a warning.)
- **`CRSPEC0011` is a warning.** A method declared on a constructed generic base type that the test assembly cannot
  call is invoked through reflection, which is not trim or AOT safe. Make the method `public` or `internal`.
- `SpecificationMethods<T, TSpecBase>.Establish`, `Because` and `Destroy` are marked `[RequiresUnreferencedCode]`: they discover
  the lifecycle methods through reflection. Trimmed or AOT callers should use the native runner, whose generated code calls
  the lifecycle methods directly.
- The runner prints a console tree and can write Markdown, JSON and GitHub summary reports; see
  [runner output and reports](./runner-output.md).

## Analyzers: one switch, one package

The analyzers `CRSPEC0001`–`CRSPEC0009` are shipped in `Cratis.Specifications` only. The adapter packages receive
them through that dependency, so they run once instead of twice. (`CRSPEC0010` and `CRSPEC0011` come from the native
runner's source generator.) `CratisSpecificationsAnalyzers` controls them for
every package:

| Value | Effect |
| --- | --- |
| unset or `true` | The analyzers run. |
| `false` | The analyzers never run. |
| `ide` | The analyzers run in IDE design-time builds only. |

`ExcludeAssets="analyzers"` on the adapter package reference no longer removes them, because they come from the
`Cratis.Specifications` dependency; use the property. See [Analyzers](../README.md#analyzers).

## Keep package versions aligned

Use the same version of `Cratis.Specifications` and of the adapter package. The assertions live in the core package and
the adapter supplies the exception type they throw, so the two are released and tested together. If a repository pins
`Cratis.Specifications` directly (for example in a `Directory.Packages.props`), update that pin together with the
adapter.

## Runtime settings and NUnit parallelism remain opt-in

No GC or PGO default changes are introduced. `CratisSpecificationsRuntimeDefaults=true` opts into
non-concurrent GC and disables dynamic PGO; `false` leaves SDK settings unchanged. Explicit
`ConcurrentGarbageCollection` and `TieredPGO` properties take precedence. NUnit fixture parallelism
also remains off unless `CratisSpecificationsParallelFixtures=true`; do not combine it with an existing
assembly-level `Parallelizable` attribute. See [Running specifications fast](https://github.com/Cratis/Specifications/blob/main/Documentation/running-fast.md)
for trade-offs and executable launch guidance.

## New in 5.0

Additive features that you do not need to adopt:

- [Shared resources and isolation leases](./shared-resources.md) (experimental): start a database or a
  container once per test session and give each specification its own lease. Opt in with
  `CratisSpecificationsSharedResources=true`; the API reports `CRSPECEXP01`. A source generator that writes `typeof(...)`
  for every public type of referenced assemblies can trip `CRSPECEXP01` in its generated code even when you do not use
  the API. Cratis Fundamentals' type discovery handles this from 7.26.1; for other generators, update them or add
  `CRSPECEXP01` to `NoWarn` in the specification project.
- [Phase timings](./phase-timings.md): see how long `Establish`, `Because` and `Destroy` take, report slow
  phases, and write a JSON timings file.
- [Runner output and reports](./runner-output.md): a console tree and Markdown, JSON and GitHub summary
  reports for the native runner.
- [The native runner](./runner.md): IDE support, options, limits and migrating from xUnit.
