// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;
using Cratis.Specifications.Resources;
using Xunit;

namespace Consumer.Resources;

public class when_leasing_a_connection_to_a_shared_server : Specification
{
    TcpProbeServer _server = null!;
    TcpProbeConnection _connection = null!;

    async Task Establish()
    {
        _server = await SpecificationResources.Get<TcpProbeServer>();
        _connection = await SpecificationResources.Lease<TcpProbeServer, TcpProbeConnection>();
    }

    [Fact] void should_start_the_server_once() => TcpProbeServer.Starts.ShouldEqual(1);
    [Fact] void should_listen_on_a_port() => _server.Port.ShouldBeGreaterThan(0);
    [Fact] void should_be_connected() => _connection.Connected.ShouldBeTrue();
    [Fact] void should_hold_the_only_connection() => _connection.HeldAtLease.ShouldEqual(1);
}
