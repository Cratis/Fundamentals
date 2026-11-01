// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_disposing_from_an_observer_callback : Specification
{
    List<Exception> failures;
    List<int> releases;
    List<int> values;

    void Because()
    {
        failures = [];
        releases = [];
        values = [];
        foreach (var scoped in new[] { false, true })
        {
            var source = new given.tracked_source();
            using var subject = scoped
                ? TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value => value)
                : new TransformingSubject<int, int>(source, value => value);
            using var consumer = subject.Subscribe(value =>
            {
                values.Add(value);
                subject.Dispose();
                subject.Dispose();
            });
            failures.Add(Catch.Exception(() => source.OnNext(1)));
            source.OnNext(2);
            releases.Add(source.releases);
        }
    }

    [Fact] void should_not_dispose_replay_on_its_own_callback_stack() => failures.ShouldEqual([null, null]);
    [Fact] void should_finish_each_admitted_callback_once() => values.ShouldEqual([1, 1]);
    [Fact] void should_release_all_owned_subscriptions_in_both_modes() => releases.ShouldEqual([2, 1]);
}
