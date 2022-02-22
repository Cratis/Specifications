# [v2.0.0] - 2021-11-22 [PR: #18](https://github.com/aksio-system/Specifications/pull/18)



## Summary

For consistency, since we don't have **Async** postfixes anywhere in our API; - removing it.

```csharp
async Task Because() => result = await Catch.Exception(() => subject.AuthenticateAsync(null, null));

[Fact] void should_throw_user_must_be_specified_exception() => result.ShouldBeOfExactType<UserMustBeSpecified>();
```

Summary of the PR here. The GitHub release description is created from this comment so keep it nice and descriptive.
Remember to remove sections that you don't need or use.
If it does not make sense to have a summary, you can take that out as well.

### Changed

- Removing **Async** postfix for handling async exceptions - for consistency.

# [v1.6.0] - 2021-11-22 [PR: #17](https://github.com/aksio-system/Specifications/pull/17)

## Summary

Introducing ability to catch exceptions from async calls in a more elegant way:

```csharp
async Task Because() => result = await Catch.ExceptionAsync(() => subject.AuthenticateAsync(null, null));

[Fact] void should_throw_user_must_be_specified_exception() => result.ShouldBeOfExactType<UserMustBeSpecified>();

```


### Added

- Adding `ExceptionAsync` method on the `Catch` class.


# [v1.5.1] - 2021-9-15 [PR: #16](https://github.com/aksio-system/Specifications/pull/16)

### Fixed

- Supporting pure synchronous lifetime method chains (Establish, Because, Destroy) - meaning that it does not create an async context if all methods of a particular type are synchronous.



# [v1.5.0] - 2021-9-15 [PR: #15](https://github.com/aksio-system/Specifications/pull/15)

### Added

- Should extension method for collections: `.ShouldEachConformTo<>()` - taking a callback for checking conformity.
- Should extension method for checking if an object matches: `.ShouldMatch<>()` - taking a callback that decides what it is expecting.



# [v1.4.4] - 2021-9-15 [PR: #14](https://github.com/aksio-system/Specifications/pull/14)

### Fixed

- Adding `PrivateAssets="All"` for the **Aksio.Defaults** package reference to avoid it bleeding its rules to consumers.


# [v1.4.3] - 2021-9-14 [PR: #13](https://github.com/aksio-system/Specifications/pull/13)

### Fixed

- Fixing package metadata for release.


# [v1.4.2] - 2021-9-14 [PR: #12](https://github.com/aksio-system/Specifications/pull/12)

### Fixed

- Static code analysis applied - fixed code that was not adhering to it.
- Fixing package metadata.



# [v1.4.1] - 2021-9-10 [PR: #11](https://github.com/aksio-system/Specifications/pull/11)

Testing out the entire pipeline

# [v1.3.0] - 2021-9-10 [PR: #10](https://github.com/aksio-system/Specifications/pull/10)

Testing

# [v1.1.0] - 2021-9-10 [PR: #9](https://github.com/aksio-system/Specifications/pull/9)

Yet another test.

# [1.0.2] - 2021-9-9 [PR: #5](https://github.com/aksio-system/Specifications/pull/5)

Initial release.

# [1.0.1] - 2021-9-9 [PR: #4](https://github.com/aksio-system/Specifications/pull/4)

Initial release.


# [1.0.0] - 2021-9-9 [PR: #3](https://github.com/aksio-system/Specifications/pull/3)

Initial release - big bang.


# [1.0.0] - 2021-9-9 [PR: #1](https://github.com/aksio-system/Specifications/pull/1)

Initial release - big bang.
