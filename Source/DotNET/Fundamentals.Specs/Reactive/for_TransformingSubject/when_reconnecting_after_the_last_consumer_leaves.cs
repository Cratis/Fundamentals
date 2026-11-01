// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_reconnecting_after_the_last_consumer_leaves : Specification
{
    given.tracked_source[] sources;
    List<int> received;
    int factories;
    int beforeFreshValue;

    void Because()
    {
        sources = [new(), new()];
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(() => sources[factories++], value => value * 2);
        subject.OnNext(90);
        using (subject.Subscribe(_ => { })) sources[0].OnNext(1);
        subject.OnNext(91);
        sources[0].OnNext(2);
        received = [];
        using var second = subject.Subscribe(received.Add);
        beforeFreshValue = received.Count;
        sources[0].OnNext(3);
        sources[1].OnNext(4);
    }

    [Fact] void should_invoke_the_factory_again() => factories.ShouldEqual(2);
    [Fact] void should_not_replay_a_previous_generation_or_idle_value() => beforeFreshValue.ShouldEqual(0);
    [Fact] void should_only_deliver_from_the_fresh_connection() => received.ShouldContainOnly(8);
    [Fact] void should_release_each_connection() => sources.Select(source => source.releases).ShouldEqual([1, 1]);
}
