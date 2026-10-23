// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Sockets;
using Cratis.Specifications.Resources;

namespace Consumer.Resources;

/// <summary>
/// A TCP server shared by every specification, standing in for a database server or a container.
/// </summary>
[SpecificationResource(Key = "tcp-probe", MaxConcurrency = 1)]
public sealed class TcpProbeServer : ISpecificationResource<TcpProbeConnection>
{
    static int _starts;
    readonly TcpListener _listener = new(IPAddress.Loopback, 0);

    public static int Starts => Volatile.Read(ref _starts);

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public ValueTask Start(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _starts);
        _listener.Start();
        return ValueTask.CompletedTask;
    }

    public async ValueTask<TcpProbeConnection> Lease(SpecificationLeaseContext context, CancellationToken cancellationToken)
    {
#pragma warning disable CA2000 // The connection owns the client from here on, and the catch disposes it when connecting fails.
        var client = new TcpClient();
#pragma warning restore CA2000
        try
        {
            await client.ConnectAsync(IPAddress.Loopback, Port, cancellationToken);
            var accepted = await _listener.AcceptTcpClientAsync(cancellationToken);
            return new(context.LeaseId, client, accepted);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        _listener.Dispose();
        return ValueTask.CompletedTask;
    }
}
