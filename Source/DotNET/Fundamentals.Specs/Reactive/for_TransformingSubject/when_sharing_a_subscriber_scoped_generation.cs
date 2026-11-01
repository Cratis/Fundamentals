// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_sharing_a_subscriber_scoped_generation : Specification
{
    given.tracked_source source;
    List<int> first;
    List<int> second;
    int factoriesBeforeSubscription;
    int factories;
    int transformations;
    int releasesBeforeLast;

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
        factoriesBeforeSubscription = factories;
        first = [];
        second = [];
        var one = subject.Subscribe(first.Add);
        source.OnNext(3);
        var two = subject.Subscribe(second.Add);
        source.OnNext(4);
        one.Dispose();
        releasesBeforeLast = source.releases;
        two.Dispose();
        two.Dispose();
    }

    [Fact] void should_not_invoke_the_factory_at_construction() => factoriesBeforeSubscription.ShouldEqual(0);
    [Fact] void should_share_one_factory_invocation() => factories.ShouldEqual(1);
    [Fact] void should_share_one_source_subscription() => source.subscriptions.ShouldEqual(1);
    [Fact] void should_transform_once_per_source_value() => transformations.ShouldEqual(2);
    [Fact] void should_deliver_live_values_to_the_first_consumer() => first.ShouldEqual([6, 8]);
    [Fact] void should_replay_one_value_to_the_second_consumer() => second.ShouldEqual([6, 8]);
    [Fact] void should_keep_the_source_until_the_last_consumer_leaves() => releasesBeforeLast.ShouldEqual(0);
    [Fact] void should_release_the_connection_once() => source.releases.ShouldEqual(1);
    [Fact] void should_not_dispose_the_source_facade() => source.sourceDisposals.ShouldEqual(0);
}
