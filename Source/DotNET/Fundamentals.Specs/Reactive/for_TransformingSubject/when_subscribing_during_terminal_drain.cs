// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_subscribing_during_terminal_drain : Specification
{
    List<int> lateValues;
    List<int> cancelledValues;
    List<string> order;
    int factories;
    int lateBeforeDrain;
    IDisposable late;

    void Because()
    {
        var source = new given.tracked_source();
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(
        () =>
        {
            factories++;
            return source;
        },
        value => value);
        lateValues = [];
        cancelledValues = [];
        order = [];
        using var first = subject.Subscribe(
        _ =>
        {
            order.Add("value");
            subject.OnCompleted();
            late = subject.Subscribe(lateValues.Add, () => order.Add("late complete"));
            using var cancelled = subject.Subscribe(cancelledValues.Add);
            lateBeforeDrain = lateValues.Count;
            order.Add("callback returned");
        },
        () => order.Add("first complete"));
        source.OnNext(1);
        late.Dispose();
    }

    [Fact] void should_not_deliver_terminal_replay_inside_the_live_callback() => lateBeforeDrain.ShouldEqual(0);
    [Fact] void should_deliver_replay_after_the_live_callback() => lateValues.ShouldContainOnly(1);
    [Fact] void should_cancel_a_pending_attachment() => cancelledValues.ShouldBeEmpty();
    [Fact] void should_finish_the_admitted_value_before_terminal_delivery() => order.ShouldEqual(["value", "callback returned", "first complete", "late complete"]);
    [Fact] void should_not_reconnect_for_a_terminal_subscriber() => factories.ShouldEqual(1);
}
