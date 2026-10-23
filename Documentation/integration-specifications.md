# Integration specifications

How to write specifications that need more than a class and its doubles: a database, a message broker, a hosted
application or an event store. This guide is a how-to with the reasoning behind each practice. It describes what you
can do today and what is planned.

## Start by not needing infrastructure

Many "integration" behaviors do not need a container. Arc and Chronicle ship in-process scenarios in their own repositories that run the real
command pipeline, projections, reducers and append path in memory. They are specifications in the sense of this
repository: fast, isolated and run with the same runners.

| Scenario | Assembly | What it runs |
| --- | --- | --- |
| `CommandScenario<TCommand>` | `Cratis.Arc.Testing` (`Cratis.Arc.Testing.Commands`) | An Arc command through its pipeline, with validation, authorization and operations. |
| `ReadModelScenario<TReadModel>` | `Cratis.Chronicle.Testing` (`Cratis.Chronicle.Testing.ReadModels`) | A projection or reducer, auto-detected, fed by events. |
| `EventScenario` | `Cratis.Chronicle.Testing` (`Cratis.Chronicle.Testing.EventSequences`) | The kernel append path on in-memory storage: constraints, serialization, migrations, PII. Observers do not run. |
| `ReactorScenario<TReactor>` | `Cratis.Chronicle.Testing` | A reactor and its recorded side effects. |

```csharp
using Cratis.Arc.Testing.Commands;

public class when_adding_an_item_to_the_cart : Specification
{
    CommandScenario<AddItemToCart> _scenario;
    CommandResult _result;

    void Establish() => _scenario = new();

    async Task Because() => _result = await _scenario.Execute(new AddItemToCart("SKU-123", 2));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
```

```csharp
using Cratis.Chronicle.Testing.ReadModels;

public class when_an_author_has_registered : Specification
{
    ReadModelScenario<AuthorOverview> _scenario;

    async Task Establish()
    {
        _scenario = new();
        await _scenario.Given.ForEventSource(AuthorId).Events(new AuthorRegistered("Jane"));
    }

    [Fact] void should_project_the_name() => _scenario.Instance.Name.ShouldEqual("Jane");
}
```

`AddItemToCart`, `AuthorOverview`, `AuthorRegistered` and `AuthorId` are your own types. Both scenarios are `IDisposable`;
dispose them in `Destroy` or implement `IDisposable` on the specification.

Use real infrastructure only for what the in-process scenarios cannot prove: a storage provider's behavior, a real
serializer round trip, network and process boundaries, migrations against a real database.

## Share infrastructure, isolate state

Starting a container costs seconds; creating a schema or namespace costs milliseconds. Pay the first once and the
second per specification.

- **Share the expensive part.** Put the container or host in a framework fixture. In xUnit, a collection fixture
  (`ICollectionFixture<T>`) shares one instance across all classes in the collection; a class fixture
  (`IClassFixture<T>`) shares it across one class. Chronicle's own integration suites use a collection fixture:

  ```csharp
  [CollectionDefinition(Name)]
  public class ChronicleCollection : ICollectionFixture<ChronicleFixture>
  {
      public const string Name = "Chronicle";
  }
  ```

  Specifications in one xUnit collection run sequentially, so shared mutable state is safe only if every
  specification isolates its own data.
- **Isolate the state.** Give each specification its own database, schema, namespace, tenant, event source id or key
  prefix, created in `Establish` and removed in `Destroy`. Chronicle's SQL Server sink fixtures create a database
  per specification on a shared container, then clear connection pools and drop it.
- **Share state only when it is immutable.** Seed reference data once in the fixture. Never share data that a
  specification changes.
- **Reuse an existing server in CI.** Accept an external connection string and fall back to starting a container.
  Chronicle's `MongoDBFixture` does this.

## Chronicle integration fixtures

`Cratis.Chronicle.XUnit.Integration` (in the Chronicle repository) supplies fixtures and base types for specifications
that run against a real Chronicle kernel and MongoDB:

- `Given<TSetup>` is an `IClassFixture<TSetup>` base for specification-style contexts; `TSetup` implements
  `IChronicleSetupFixture`, which exposes the event store, client, services and the MongoDB databases.
- `Specification<TChronicleFixture>` runs your `Establish`, `Because` and `Destroy` conventions inside the fixture
  lifecycle.
- `ChronicleInProcessFixture` and `ChronicleOutOfProcessFixture` host the kernel in or out of the test process.

Read the package source in the
[Chronicle repository](https://github.com/Cratis/Chronicle/tree/main/Source/Clients/XUnit.Integration) before adopting
it; it is xUnit-specific and its surface changes with Chronicle.

## A fixture sketch

The shape is the same for any container: start once, expose a connection, create isolated state per specification,
bound every wait.

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    readonly PostgreSqlContainer _container = new PostgreSqlBuilder().Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}
```

This uses [Testcontainers](https://dotnet.testcontainers.org) and xUnit v3's `IAsyncLifetime` returning `ValueTask`;
on xUnit v2 it returns `Task`. A specification joins the collection with `[Collection(DatabaseCollection.Name)]` and
receives the fixture through its constructor. Constructor arguments that are framework-managed fixtures keep the class
eligible for the [shared lifecycle](https://github.com/Cratis/Specifications/blob/main/README.md#shared-instance-per-specification-class).

## Keep them fast and reliable

- **Never sleep.** Wait on a condition with a deadline: poll a readiness endpoint, await the event that signals
  completion, or use the scenario's own wait. A fixed `Task.Delay` is both slow and flaky.
- **Make readiness explicit.** A started container is not a ready service. Probe it (a query, a health endpoint)
  before the fixture reports ready, and fail with the container's logs if it does not.
- **Bound cleanup.** Every Docker, network and teardown call has a timeout. A hung `DisposeAsync` holds the suite;
  log and move on when cleanup fails.
- **Start nothing for skipped specifications.** Create infrastructure lazily in the fixture and keep integration
  specifications in their own project, so unit runs and filtered runs never start a container.
- **Keep facts read-only.** The rule from the shared lifecycle applies here too: infrastructure changes happen in
  `Establish` and `Because`, never in a fact.
- **Separate the lanes.** Run unit specifications on every change and integration projects as a separate, shardable
  job. See [Running specifications fast](./running-fast.md).

## Where this is heading

Two proposals are planned and not shipped, so nothing here can be used yet:

- Shared resources and isolation leases (#75): a portable way to declare a resource once, acquire it across
  frameworks and release it in reverse order.
- Phase timings (#76): reporting how long acquisition, startup, reset, setup, the action and cleanup take, so slow
  infrastructure is visible.

Until they exist, use your framework's fixtures as described above.

## Related

- [Structuring specifications](./structuring-specifications.md)
- [Running specifications fast](./running-fast.md)
