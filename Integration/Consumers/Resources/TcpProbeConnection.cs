// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;

namespace Consumer.Resources;

/// <summary>
/// An exclusive connection to the shared <see cref="TcpProbeServer"/>, held by one specification at a time.
/// </summary>
/// <param name="leaseId">The identifier of the lease.</param>
/// <param name="client">The client end of the connection.</param>
/// <param name="accepted">The server end of the connection.</param>
public sealed class TcpProbeConnection(string leaseId, TcpClient client, TcpClient accepted) : IAsyncDisposable
{
    static int _held;

    readonly int _heldAtLease = Interlocked.Increment(ref _held);

    public string LeaseId { get; } = leaseId;

    public bool Connected => client.Connected && accepted.Connected;

    public int HeldAtLease => _heldAtLease;

    public ValueTask DisposeAsync()
    {
        Interlocked.Decrement(ref _held);
        client.Dispose();
        accepted.Dispose();
        return ValueTask.CompletedTask;
    }
}
