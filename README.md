# Specifications

[![Build](https://github.com/aksio-system/Specifications/actions/workflows/build.yml/badge.svg)](https://github.com/aksio-system/Specifications/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/aksio.specifications)](http://nuget.org/packages/aksio.specifications)

This project represents a way to do Specification by Example - BDD style inspired by
the conciseness of [Machine.Specifications](https://github.com/machine/machine.specifications).
The motivation behind is years of work with Machine.Specifications and the wish to maintain
the approach, structure and syntax - but get more love from tooling across different platforms
and IDEs and editors; read more [here](https://www.ingebrigtsen.info/2021/09/05/specifications-in-xunit/).

## What does it do?

In BDD one talks about the **given**, **when**, **then**. Much like **arrange**, **act** and **assert** in a way that
is more common in TDD. The biggest difference is on a mindset level of thinking in specifications of behaviors in your
system. What this particular library delivers is a way to do these and also keep in line with what is common in the BDD
world of having isolated specifications and not have typically a **FooTests** and dump all your tests for the unit `Foo` in
it.

The library supports my convention lifecycle methods `Establish()`, `Because()` and `Destroy()`. There is no virtual method
to override, just match the expected signatures:

| Signature | Purpose |
| --------- | ------- |
| void Establish() | Establishes the current context - **given** / **arrange** |
| void Because() | Triggers the behavior being specified - **when** / **act** |
| void Destroy() | Tears down the context |

If your specification requires to run in an async context, it also supports the following:

| Signature | Purpose |
| --------- | ------- |
| Task Establish() | Establishes the current context - **given** / **arrange** |
| Task Because() | Triggers the behavior being specified - **when** / **act** |
| Task Destroy() | Tears down the context |

All lifecycle methods are optional and will be ignored if not there.
Multiple levels of inheritance recursively is supported, meaning that specifications will run all the lifecycle methods
from the lowest level in the hierarchy chain and up the hierarchy (e.g. no_user_authenticated -> when_authenticating_a_null_user).

To get all this to work, all you need to do is inherit from the `Specification` type found in `Aksio.Specifications`.

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

| Warning | Description |
| ------- | ----------- |
| [CA1707](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1707) | Identifiers should not contain underscores |
| [CA1051](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/CA1051) | Do not declare visible instance fields |
| [CS1591](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591)  | Missing XML comment for publicly visible type or member 'Type_or_Member' |
| [IDE0051](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0051) | Remove unused private member |
| [IDE1006](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/naming-rules#rule-id-ide1006-naming-rule-violation) | Naming rule violation |
| [RCS1213](https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1213.md) | Remove unused member declaration|

## Example

In the [sample](./Sample) folder you'll find samples of using it.
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
