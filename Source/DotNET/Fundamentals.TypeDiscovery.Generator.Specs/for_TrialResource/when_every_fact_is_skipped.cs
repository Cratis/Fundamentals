// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_every_fact_is_skipped : Specification
{
    /// <summary>
    /// Gets a value indicating whether to skip every fact; Establish still runs (Cratis/Specifications#99).
    /// </summary>
    public static bool IsAlwaysSkipped => true;

    async Task Establish()
    {
        if (!ProbeGate.IsOn) return;
        await SpecificationResources.Get<SkippedProbe>().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact(Skip = "Proves a skipped specification starts no shared resource (Cratis/Fundamentals#1210)", SkipWhen = nameof(IsAlwaysSkipped))] void should_start_no_resource() => ProbeLedger.StartsOf<SkippedProbe>().ShouldEqual(0);
}
