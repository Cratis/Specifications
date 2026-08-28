# Cratis Specifications

**Specification by Example (BDD) for .NET — Given/When/Then specs with xUnit and NUnit, in the style of [Machine.Specifications](https://github.com/machine/machine.specifications).**

[![Build](https://github.com/Cratis/Specifications/actions/workflows/build.yml/badge.svg)](https://github.com/Cratis/Specifications/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/cratis.specifications.xunit?label=Cratis.Specifications.XUnit&logo=nuget)](https://www.nuget.org/packages/cratis.specifications.xunit)
[![Nuget](https://img.shields.io/nuget/v/cratis.specifications.nunit?label=Cratis.Specifications.NUnit&logo=nuget)](https://www.nuget.org/packages/cratis.specifications.nunit)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Discord](https://img.shields.io/discord/1182595891576717413?label=Discord&logo=discord&logoColor=white)](https://discord.gg/kt4AMpV8WV)

Cratis Specifications brings Specification by Example — behavior-driven development (BDD) with
**given**, **when**, **then** — to .NET test projects, keeping the concise structure and syntax
popularized by Machine.Specifications while running on standard test frameworks. That means full
tooling support across IDEs, editors, and CI, with your choice of xUnit or NUnit as the runner.
The background and motivation are described in
[this article](https://web.archive.org/web/20210922202940/https://www.ingebrigtsen.info/2021/09/05/specifications-in-xunit/).

Packages:

- [Cratis.Specifications.XUnit](https://www.nuget.org/packages/cratis.specifications.xunit)
- [Cratis.Specifications.NUnit](https://www.nuget.org/packages/cratis.specifications.nunit)

## What does it do?

In BDD one talks about the **given**, **when**, **then**. Much like **arrange**, **act** and **assert** in a way that
is more common in TDD. The biggest difference is on a mindset level of thinking in specifications of behaviors in your
system. What this particular library delivers is a way to do these and also keep in line with what is common in the BDD
world of having isolated specifications and not have typically a **FooTests** and dump all your tests for the unit `Foo` in
it.

The library supports the convention lifecycle methods `Establish()`, `Because()` and `Destroy()`. There is no virtual method
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

All lifecycle methods are optional and will be ignored if not there.
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

In the [sample](./Sample) folder you'll find samples of using it both for XUnit and NUnit.

The difference between XUnit and NUnit is on the signature of the tests.
Instead of `[Fact]` for XUnit, you'll have to adorn with `[Test]`, in addition to that, NUnit
requires methods to be public to run them.

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
