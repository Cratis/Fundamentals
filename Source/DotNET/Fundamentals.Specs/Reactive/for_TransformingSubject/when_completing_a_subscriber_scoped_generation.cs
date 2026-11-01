// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_completing_a_subscriber_scoped_generation : Specification
{
    given.tracked_source source;
    List<int> lateValues;
    int factories;
    int completions;
    int lateCompletions;
    int transformations;

    void Because()
    {
        source = new();
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(
        () =>
        {
            factories++;
            return source;
        },
        value =>
        {
            transformations++;
            return value * 2;
        });
        using var first = subject.Subscribe(_ => { }, () => completions++);
        source.OnNext(2);
        source.OnCompleted();
        subject.OnNext(999);
        subject.OnCompleted();
        subject.OnError(new Exception());
        lateValues = [];
        using var late = subject.Subscribe(lateValues.Add, () => lateCompletions++);
    }

    [Fact] void should_complete_the_current_consumer_once() => completions.ShouldEqual(1);
    [Fact] void should_replay_the_last_transformed_value_to_a_late_consumer() => lateValues.ShouldContainOnly(4);
    [Fact] void should_complete_the_late_consumer_once() => lateCompletions.ShouldEqual(1);
    [Fact] void should_not_reconnect_after_completion() => factories.ShouldEqual(1);
    [Fact] void should_not_transform_after_completion() => transformations.ShouldEqual(1);
    [Fact] void should_release_the_upstream_connection_once() => source.releases.ShouldEqual(1);
}
