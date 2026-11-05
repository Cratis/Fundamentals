// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_a_resource_fails_part_way_through_starting : Specification
{
    Exception _first;
    Exception _second;

    async Task Because()
    {
        if (!ProbeGate.IsOn) return;
        _first = await Catch.Exception(async () => await SpecificationResources.Get<PartiallyStartingProbe>().AsTask().WaitAsync(TimeSpan.FromSeconds(10)));
        _second = await Catch.Exception(async () => await SpecificationResources.Get<PartiallyStartingProbe>().AsTask().WaitAsync(TimeSpan.FromSeconds(10)));
    }

    [ProbeFact] void should_fail_with_the_start_failure() => _first.ShouldBeOfExactType<InvalidOperationException>();
    [ProbeFact] void should_fail_later_requests_with_the_same_failure() => _second.ShouldBeSame(_first);
    [ProbeFact] void should_start_once() => ProbeLedger.StartsOf<PartiallyStartingProbe>().ShouldEqual(1);
    [ProbeFact] void should_dispose_the_partly_started_resource() => PartiallyStartingProbe.DisposeCalls.ShouldEqual(1);
    [ProbeFact] void should_stop_its_listener() => PartiallyStartingProbe.ListenerStopped.ShouldBeTrue();
}
