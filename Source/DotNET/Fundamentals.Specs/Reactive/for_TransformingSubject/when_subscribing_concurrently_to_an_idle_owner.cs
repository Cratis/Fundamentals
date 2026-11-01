// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_subscribing_concurrently_to_an_idle_owner : Specification
{
    given.tracked_source source;
    int factories;
    int firstValues;
    int secondValues;

    async Task Because()
    {
        source = new();
        using var entered = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(
        () =>
        {
            Interlocked.Increment(ref factories);
            entered.Set();
            if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            return source;
        },
        value => value);
        var first = Task.Run(() => subject.Subscribe(_ => Interlocked.Increment(ref firstValues)));
        IDisposable? second = null;
        try
        {
            if (!entered.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            second = subject.Subscribe(_ => Interlocked.Increment(ref secondValues));
        }
        finally { resume.Set(); }
        using var firstConsumer = await first.WaitAsync(TimeSpan.FromSeconds(10));
        using var secondConsumer = second;
        source.OnNext(1);
    }

    [Fact] void should_invoke_the_factory_once() => factories.ShouldEqual(1);
    [Fact] void should_subscribe_to_the_source_once() => source.subscriptions.ShouldEqual(1);
    [Fact] void should_deliver_to_the_first_reservation() => firstValues.ShouldEqual(1);
    [Fact] void should_deliver_to_the_second_reservation() => secondValues.ShouldEqual(1);
    [Fact] void should_release_the_shared_connection_once() => source.releases.ShouldEqual(1);
}
