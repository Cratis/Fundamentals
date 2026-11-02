// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_TrialResource;

public sealed class when_waiting_for_a_slow_start_is_cancelled : Specification
{
    Exception _error;
    SlowStartingProbe _probe;

    async Task Because()
    {
        if (!ProbeGate.IsOn) return;
        using var cancellation = new CancellationTokenSource();
        var waiting = SpecificationResources.Get<SlowStartingProbe>(cancellation.Token).AsTask();
        await SlowStartingProbe.Starting.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await cancellation.CancelAsync();
        _error = await Catch.Exception(() => waiting.WaitAsync(TimeSpan.FromSeconds(10)));
        SlowStartingProbe.Release();
        _probe = await SpecificationResources.Get<SlowStartingProbe>().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }

    [ProbeFact] void should_fail_the_waiting_request_as_cancelled() => _error.ShouldBeOfExactType<SpecificationCancelledException>();
    [ProbeFact] void should_finish_starting_for_later_requests() => _probe.IsStarted.ShouldBeTrue();
    [ProbeFact] void should_start_once() => ProbeLedger.StartsOf<SlowStartingProbe>().ShouldEqual(1);
}
