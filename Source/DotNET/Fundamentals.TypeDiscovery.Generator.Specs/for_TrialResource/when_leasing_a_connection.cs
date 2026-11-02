// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_leasing_a_connection : given.a_leased_connection
{
    string _reply;

    async Task Because()
    {
        if (!ProbeGate.IsOn) return;
        _reply = await _connection.Echo(_connection.LeaseId, CancellationToken.None);
    }

    [ProbeFact] void should_start_the_server_once() => ProbeLedger.StartsOf<TcpProbeServer>().ShouldEqual(1);
    [ProbeFact] void should_get_its_own_lease_id_echoed() => _reply.ShouldEqual(_connection.LeaseId);
    [ProbeFact] void should_have_a_lease_id_no_other_lease_had() => _connection.HasNewLeaseId.ShouldBeTrue();
}
