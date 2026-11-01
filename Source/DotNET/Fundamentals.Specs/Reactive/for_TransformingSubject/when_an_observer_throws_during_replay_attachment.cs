// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_an_observer_throws_during_replay_attachment : Specification
{
    given.tracked_source source;
    Exception failure;
    readonly Exception expected = new("observer failed");
    int releasesWithFirstConsumer;

    void Because()
    {
        source = new();
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value => value);
        var first = subject.Subscribe(_ => { });
        source.OnNext(1);
        failure = Catch.Exception(() => subject.Subscribe(_ => throw expected));
        releasesWithFirstConsumer = source.releases;
        first.Dispose();
    }

    [Fact] void should_propagate_the_observer_exception() => failure.ShouldBeSame(expected);
    [Fact] void should_not_disconnect_the_remaining_consumer() => releasesWithFirstConsumer.ShouldEqual(0);
    [Fact] void should_roll_back_the_failed_consumer_reservation() => source.releases.ShouldEqual(1);
}
