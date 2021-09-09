# Specifications

[![Build](https://github.com/aksio-system/Specifications/actions/workflows/build.yml/badge.svg)](https://github.com/aksio-system/Specifications/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/aksio.specifications)](http://nuget.org/packages/aksio.specifications)

This project represents a way to do Specification by Example - BDD style inspired by
the conciseness of [Machine.Specifications](https://github.com/machine/machine.specifications).
The motivation behind is years of work with Machine.Specifications and the wish to maintain
the approach, structure and syntax - read more [here](https://www.ingebrigtsen.info/2021/09/05/specifications-in-xunit/).

## Using

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
