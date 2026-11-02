// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource.given;

public abstract class a_leased_connection : Specification
{
    private protected TcpProbeConnection _connection;

    async Task Establish()
    {
        if (!ProbeGate.IsOn) return;
        _connection = await SpecificationResources.Lease<TcpProbeServer, TcpProbeConnection>().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }
}
