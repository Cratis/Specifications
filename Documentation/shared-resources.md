# Shared resources

How to share an expensive resource, such as a database server, a container or a compiled set of inputs, between
specifications, and how to give each specification its own isolated part of it.

> The shared resources API is experimental. Using it reports the diagnostic `CRSPECEXP01`; suppress it to opt in:
> `<NoWarn>$(NoWarn);CRSPECEXP01</NoWarn>`. The API can change before it graduates.

## Define a resource

Implement `ISpecificationResource` from `Cratis.Specifications.Resources`. `Start` starts it; `DisposeAsync` stops it.

```csharp
public sealed class PostgresServer : ISpecificationResource
{
    public string ConnectionString { get; private set; } = string.Empty;

    public async ValueTask Start(CancellationToken cancellationToken)
    {
        // Start the container and wait until it accepts connections.
    }

    public async ValueTask DisposeAsync()
    {
        // Stop the container.
    }
}
```

Get it from `Establish`:

```csharp
public class when_reading_an_order : Specification
{
    PostgresServer _server;

    async Task Establish() => _server = await SpecificationResources.Get<PostgresServer>();
}
```

- **Lazy.** Nothing starts until a specification asks for it. A class that is skipped, filtered out or never asks
  starts nothing.
- **Once.** The resource is created and started once per test session, however many specifications ask for it at the
  same time. Every caller gets the same instance.
- **Stopped at the end of the session**, the last started first, within 30 seconds each.
- A resource without a parameterless constructor is created by a factory:
  `SpecificationResources.Get(() => new PostgresServer("postgres:17"))`. The factory is only used the first time.

## Give each specification its own lease

Leasing needs the test project to opt in, so a project that does not lease pays nothing for it:

```xml
<PropertyGroup>
    <CratisSpecificationsSharedResources>true</CratisSpecificationsSharedResources>
</PropertyGroup>
```

The property compiles a small module initializer into the test project. It makes the specifications of that project
release their leases after `Destroy`. Without it, `Lease` throws an `InvalidOperationException` that names the
property, before anything is created or started. `Get` and `Borrow` work without it.

Implement `ISpecificationResource<TLease>` to hand each specification an isolated part of the shared resource, for
example a fresh database on a shared server:

```csharp
public sealed class PostgresServer : ISpecificationResource<FreshDatabase>
{
    // Start and DisposeAsync as above.

    public async ValueTask<FreshDatabase> Lease(SpecificationLeaseContext context, CancellationToken cancellationToken)
    {
        // context.LeaseId is unique within the session; use it to name the database.
        return await FreshDatabase.Create(ConnectionString, context.LeaseId, cancellationToken);
    }
}
```

```csharp
async Task Establish() => _database = await SpecificationResources.Lease<PostgresServer, FreshDatabase>();
```

- Lease from `Establish` or `Because`. Outside the lifecycle of a specification nothing can release the lease, so
  leasing there throws `InvalidOperationException`.
- The lease is disposed automatically after `Destroy`, the most recent lease first. You do not dispose it yourself.
- `SpecificationLeaseContext` tells the resource which specification the lease is for: its type, display name and a
  lease id unique within the session.

## Limit concurrency

Put `[SpecificationResource]` on the resource to limit how many specifications hold a lease at once:

```csharp
[SpecificationResource(MaxConcurrency = 1)]
public sealed class SharedBrowser : ISpecificationResource<BrowserPage> { ... }
```

| Property | Default | Meaning |
| --- | --- | --- |
| `MaxConcurrency` | `0` | Leases held at once: `0` for no limit, `1` for exclusive access. A specification waits for a free lease. |
| `Key` | the resource type | Resources with the same key share one limit, for example two resources that use the same fixed port. |
| `StopTimeoutSeconds` | `30` | How long stopping may take before it is reported as a failure. |

Resources sharing a key must declare the same `MaxConcurrency`.

When a specification leases from more than one limited group, it must lease them in ordinal order of their keys, at
most once per group. Leasing out of order throws `SpecificationResourceOrderException` instead of risking a deadlock
with another specification that leases the same groups in the other order.

## Borrow a resource someone else owns

When something else already owns and started the resource, for example a fixture of the test framework, share it
without handing over its lifetime:

```csharp
SpecificationResources.Borrow(server);
```

A borrowed resource is never started, stopped or disposed by Cratis Specifications.

## Failures

| What fails | What happens |
| --- | --- |
| `Start` throws | The resource is disposed straight away. The specification asking for it fails in `Establish`, and so does every later one: the same exception, with no second start. |
| `Lease` throws | Only the specification that asked for the lease fails. Its lease slot is freed. |
| Disposing a lease throws | The specification that held it fails when its scope ends. Its lease slot is freed. |
| Stopping a resource throws or times out | The test framework reports it as a cleanup failure of the run, so the run does not pass. |
| Waiting is cancelled | When the `CancellationToken` passed to `Get` or `Lease` is cancelled while waiting for a start or a free lease, the call throws `SpecificationCancelledException`. The start itself carries on for the other specifications. |

If the process crashes, the session never ends and nothing is stopped; rely on the operating system or a container
reaper, such as Testcontainers' Ryuk, to clean up.

## Limits

- Resources live for the whole test session of one process. There is no class lifetime: for a resource that belongs to
  one class, set a field in `Establish`.
- Nothing is coordinated across processes or shards.
- Resetting a particular store between specifications, such as with Respawn or database templates, is up to the
  resource's `Lease`.
