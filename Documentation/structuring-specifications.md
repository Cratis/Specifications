# Structuring specifications

This guide grows one specification from a single behavior to a full folder of contexts, in six levels. Stop at the
level your behavior needs. The conventions come from the README's
[structure and naming](https://github.com/Cratis/Specifications/blob/main/README.md#structure-and-naming) section.

The examples use xUnit, so facts carry `[Fact]`. NUnit and TUnit use `[Test]` and public methods; the native runner
needs no attribute. The examples use a small `Author` domain:

```csharp
public class Author(string name)
{
    public string Name { get; } = name;
    public bool IsRegistered { get; private set; }
    public void Register() => IsRegistered = !string.IsNullOrWhiteSpace(Name);
}
```

## Level 1: a single behavior

One file, one behavior. The folder is `for_<unit>`, the file `when_<behavior>`, and the class derives from
`Specification`.

```csharp
// for_Author/when_registering.cs
namespace MyApp.for_Author;

public class when_registering : Specification
{
    Author _author;

    void Establish() => _author = new("Jane");

    void Because() => _author.Register();

    [Fact] void should_be_registered() => _author.IsRegistered.ShouldBeTrue();
    [Fact] void should_keep_the_name() => _author.Name.ShouldEqual("Jane");
}
```

- `Establish` sets up the context, `Because` triggers the behavior, `should_` facts assert. `Given` and `When` are
  aliases; a class declares one name per step, never both.
- Facts are expression-bodied, with no blank lines between them.
- Mistake: act in `Establish`, or assert in `Because`. `Because` is the single action under test.
- Mistake: several behaviors in one class. A fact that needs a different action belongs in another `when_` class.

## Level 2: several outcomes of one behavior

When a behavior branches, give each outcome its own file in a `when_<behavior>` folder, named with `and_` or `with_`.

```text
for_Author/
  when_registering/
    with_a_name.cs
    and_the_name_is_empty.cs
```

```csharp
// for_Author/when_registering/and_the_name_is_empty.cs
namespace MyApp.for_Author.when_registering;

public class and_the_name_is_empty : Specification
{
    Author _author;

    void Establish() => _author = new(string.Empty);

    void Because() => _author.Register();

    [Fact] void should_not_be_registered() => _author.IsRegistered.ShouldBeFalse();
}
```

The folder reads as a sentence: *when registering, with a name* / *and the name is empty*. The analyzer `CRSPEC0002`
keeps one specification per file.

- Mistake: one class with `if` branches or theories standing in for outcomes. Separate classes keep each failure
  report specific.

## Level 3: shared setup in `given/`

When outcomes share setup, move it into a reusable context in a `given` namespace and inherit it. Layer
base-first: dependencies, then the system under test.

```csharp
// for_AuthorRegistrar/given/all_dependencies.cs
namespace MyApp.for_AuthorRegistrar.given;

public class all_dependencies : Specification
{
    protected IAuthors _authors;

    void Establish() => _authors = Substitute.For<IAuthors>();
}
```

```csharp
// for_AuthorRegistrar/given/an_author_registrar.cs
namespace MyApp.for_AuthorRegistrar.given;

public class an_author_registrar : all_dependencies
{
    protected AuthorRegistrar _registrar;

    void Establish() => _registrar = new(_authors);
}
```

```csharp
// for_AuthorRegistrar/when_registering/with_a_name.cs
namespace MyApp.for_AuthorRegistrar.when_registering;

public class with_a_name : given.an_author_registrar
{
    void Because() => _registrar.Register("Jane");

    [Fact] void should_add_the_author() => _authors.Received(1).Add(Arg.Is<Author>(_ => _.Name == "Jane"));
}
```

- Lifecycle methods run base-first across the whole chain, so `all_dependencies.Establish` runs before
  `an_author_registrar.Establish`. Never call `base.Establish()` (`CRSPEC0005`).
- A `given` context holds `Establish` only. Put `Because` in the `when_` class (`CRSPEC0007`) and facts in the `when_`
  class; a fact on a `given` context never runs (`CRSPEC0003`).
- Mistake: a deep chain that nobody can read. Two or three layers is usually enough; past that, a context is doing
  too much.

## Level 4: collaborators, doubles and exceptions

Specifications does not ship a mocking library (add `using NSubstitute;` to the files that use it). The examples use [NSubstitute](https://nsubstitute.github.io), but any
double library works. Substitute the collaborators in `all_dependencies`, set up returns in `Establish` and verify
the interaction in a fact (see Level 3).

Catch an exception in `Because` with `Catch.Exception`, then assert on it:

```csharp
// for_AuthorRegistrar/when_registering/with_a_duplicate_name.cs
namespace MyApp.for_AuthorRegistrar.when_registering;

public class with_a_duplicate_name : given.an_author_registrar
{
    Exception _result;

    void Establish() => _authors.Exists("Jane").Returns(true);

    void Because() => _result = Catch.Exception(() => _registrar.Register("Jane"));

    [Fact] void should_throw_author_already_exists() => _result.ShouldBeOfExactType<AuthorAlreadyExists>();
    [Fact] void should_not_add_the_author() => _authors.DidNotReceive().Add(Arg.Any<Author>());
}
```

- `Catch.Exception(Action)` catches any exception, `Catch.Exception<T>(Action)` catches only `T` (others propagate),
  and `Catch.Exception(Func<Task>)` awaits async code. All return `null` when nothing is thrown, and
  `ShouldBeOfExactType` then fails with a readable message.
- Mistake: `Assert.Throws` inside a fact. Catch in `Because` so the fact only asserts.
- Mistake: verifying every call on a double. Verify the calls that are the behavior; stub the rest.

## Level 5: async work, cancellation, disposal and fixtures

All lifecycle methods can return `Task` or `ValueTask`, and a specification can implement `IDisposable` or
`IAsyncDisposable`.

```csharp
public class when_registering_asynchronously : Specification, IAsyncDisposable
{
    IAuthors _authors;
    AuthorRegistrar _registrar;
    Author _result;

    Task Establish()
    {
        _authors = Substitute.For<IAuthors>();
        _registrar = new(_authors);
        return Task.CompletedTask;
    }

    async Task Because() => _result = await _registrar.Register("Jane", CancellationToken.None);

    [Fact] void should_return_the_author() => _result.ShouldNotBeNull();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
```

- When the instance is shared (an eligible xUnit specification, which is the default, or any other package),
  `Destroy` runs once after the last fact, then disposal runs once, even if `Destroy` fails. With xUnit's per-fact
  lifecycle (`[PerFactSpecificationLifecycle]`) every fact gets its own instance, so `Destroy` and disposal run after each fact. On TUnit, keep cleanup in
  `Destroy` and do not implement `IDisposable`/`IAsyncDisposable`: TUnit disposes the shared instance after each fact.
- In the native runner, pass `SpecificationContext.CancellationToken` instead of `CancellationToken.None`. It flows
  across awaits and is isolated per class; outside a running class it is `CancellationToken.None`. Cancellation is
  cooperative. The other packages have no `SpecificationContext`.
- xUnit class and collection fixtures stay owned by xUnit and can be constructor arguments of an eligible
  specification. See the README on the
  [shared instance](https://github.com/Cratis/Specifications/blob/main/README.md#shared-instance-per-specification-class).
- Under a shared lifecycle, `Establish` and `Because` run once and facts may run concurrently. Facts must be read-only:
  no assignments to fields or to objects reached through them, no `Add` or `Clear` on collections, no `Dispose`, no
  `Returns` on a shared substitute, and no running the action again. Put every mutation in `Establish` or `Because`.
  `CRSPEC0008` reports provable violations as a build error; `CRSPEC0012` warns about calls such as `Execute`,
  `Handle` or `Dequeue` that probably change the context.
- Mistake: `async void` lifecycle methods. Return `Task`. The native runner reports them as the error `CRSPEC0010`.
- Mistake: `Task.Delay` to wait for a result. Await the operation that produces it.

## Level 6: readable names, aliases and analyzers

- Names read as sentences. By default (turn off with `<CratisSpecificationsReadableNames>false</CratisSpecificationsReadableNames>`),
  `for_Author.when_registering.should_keep_the_name` shows as
  `Author › when registering › should keep the name`. Names are code names, so choose them for the reader: a past or
  present-tense behavior in `when_`, an outcome in `should_`.
- `Given` and `When` suit teams that think in given/when/then; `Establish` and `Because` suit teams coming from
  Machine.Specifications. Pick one for the project and keep to it. Mixing across base and derived contexts works;
  both names on one class throws `AmbiguousLifecycleMethod` (`CRSPEC0009`).
- The analyzers `CRSPEC0001` to `CRSPEC0009` and `CRSPEC0012` keep specifications honest: `should_` naming, one specification per file,
  public specification classes, no facts or `Because` on a `given` context, no explicit base lifecycle calls, no
  mutation of shared context, and no ambiguous aliases. See the README on
  [analyzers](https://github.com/Cratis/Specifications/blob/main/README.md#analyzers), including how to lower
  `CRSPEC0008` from an error or turn the analyzers off.

## Where next

- [Running specifications fast](./running-fast.md)
- [Integration specifications](./integration-specifications.md)
