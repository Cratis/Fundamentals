// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_the_probe_is_off : Specification
{
    [ProbeFact(ProbeRun.Off)] void should_start_no_probe_resource() => ProbeLedger.TotalStarts.ShouldEqual(0);
    [ProbeFact(ProbeRun.Off)] void should_lease_nothing() => ProbeLedger.Leases.ShouldEqual(0);
}
