// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_leasing_an_exclusive_connection : Specification
{
    TcpProbeConnection _connection;
    string _reply;

    async Task Establish()
    {
        if (!ProbeGate.IsOn) return;
        _connection = await SpecificationResources.Lease<ExclusiveTcpProbeServer, TcpProbeConnection>().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }

    async Task Because()
    {
        if (!ProbeGate.IsOn) return;
        _reply = await _connection.Echo(_connection.LeaseId, CancellationToken.None);
    }

    [ProbeFact] void should_hold_the_only_lease_on_the_key() => _connection.HeldAtLease.ShouldEqual(1);
    [ProbeFact] void should_start_the_server_once() => ProbeLedger.StartsOf<ExclusiveTcpProbeServer>().ShouldEqual(1);
    [ProbeFact] void should_be_connected() => _reply.ShouldEqual(_connection.LeaseId);
}
