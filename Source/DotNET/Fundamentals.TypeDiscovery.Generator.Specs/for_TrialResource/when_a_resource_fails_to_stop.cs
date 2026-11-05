// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_a_resource_fails_to_stop : Specification
{
    async Task Establish()
    {
        if (!ProbeGate.IsTeardownFailure) return;
        await SpecificationResources.Get<FailingStopProbe>().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }

    [ProbeFact(ProbeRun.TeardownFailure)] void should_start_the_resource() => ProbeLedger.StartsOf<FailingStopProbe>().ShouldEqual(1);
}
