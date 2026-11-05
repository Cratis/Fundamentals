// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_a_lease_is_requested_with_a_cancelled_token : Specification
{
    Exception _error;
    TcpProbeConnection _connection;

    async Task Establish()
    {
        if (!ProbeGate.IsOn) return;
        await SpecificationResources.Get<ExclusiveTcpProbeServer>().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }

    async Task Because()
    {
        if (!ProbeGate.IsOn) return;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        _error = await Catch.Exception(async () => await SpecificationResources.Lease<ExclusiveTcpProbeServer, TcpProbeConnection>(cancellation.Token).AsTask().WaitAsync(TimeSpan.FromSeconds(10)));
        _connection = await SpecificationResources.Lease<ExclusiveTcpProbeServer, TcpProbeConnection>().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }

    [ProbeFact] void should_fail_as_cancelled() => _error.ShouldBeOfExactType<SpecificationCancelledException>();
    [ProbeFact] void should_lease_afterwards() => _connection.ShouldNotBeNull();
    [ProbeFact] void should_hold_the_only_lease_on_the_key() => _connection.HeldAtLease.ShouldEqual(1);
}
