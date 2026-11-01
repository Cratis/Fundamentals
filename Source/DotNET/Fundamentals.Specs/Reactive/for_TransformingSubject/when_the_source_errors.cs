// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_the_source_errors : Specification
{
    List<Exception> currentErrors;
    List<Exception> lateErrors;
    List<int> lateValues;
    List<int> releases;
    List<int> releasesBeforeOwnerDisposal;
    readonly Exception expected = new("source failed");

    void Because()
    {
        currentErrors = [];
        lateErrors = [];
        lateValues = [];
        releases = [];
        releasesBeforeOwnerDisposal = [];
        foreach (var scoped in new[] { false, true })
        {
            var source = new given.tracked_source();
            using var subject = scoped
                ? TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value => value * 2)
                : new TransformingSubject<int, int>(source, value => value * 2);
            using var first = subject.Subscribe(_ => { }, currentErrors.Add);
            source.OnNext(1);
            source.OnError(expected);
            using var late = subject.Subscribe(lateValues.Add, lateErrors.Add);
            releasesBeforeOwnerDisposal.Add(source.releases);
            subject.Dispose();
            releases.Add(source.releases);
        }
    }

    [Fact] void should_deliver_the_original_error_in_both_modes() => currentErrors.ShouldEqual([expected, expected]);
    [Fact] void should_preserve_the_error_for_late_consumers_in_both_modes() => lateErrors.ShouldEqual([expected, expected]);
    [Fact] void should_preserve_the_last_value_for_late_consumers_in_both_modes() => lateValues.ShouldEqual([2, 2]);
    [Fact] void should_preserve_the_default_completion_monitor_until_owner_disposal() => releasesBeforeOwnerDisposal.ShouldEqual([1, 1]);
    [Fact] void should_release_all_owned_connections_on_explicit_disposal_in_both_modes() => releases.ShouldEqual([2, 1]);
}
