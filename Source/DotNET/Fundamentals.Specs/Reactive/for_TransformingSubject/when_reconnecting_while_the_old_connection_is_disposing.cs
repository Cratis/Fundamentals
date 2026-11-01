// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_reconnecting_while_the_old_connection_is_disposing : Specification
{
    given.tracked_source[] sources;
    List<int> values;
    int factories;
    int liveReleasesDuringOldDisposal;

    async Task Because()
    {
        sources = [new(), new()];
        using var oldDisposing = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        sources[0].onRelease = () =>
        {
            oldDisposing.Set();
            if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
        };
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(() => sources[Interlocked.Increment(ref factories) - 1], value => value);
        var first = subject.Subscribe(_ => { });
        sources[0].OnNext(1);
        var retiring = Task.Run(first.Dispose);
        values = [];
        try
        {
            if (!oldDisposing.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            using var next = subject.Subscribe(values.Add);
            sources[0].OnNext(2);
            sources[1].OnNext(3);
            liveReleasesDuringOldDisposal = sources[1].releases;
        }
        finally { resume.Set(); }
        await retiring.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact] void should_create_an_independent_connection_without_waiting() => factories.ShouldEqual(2);
    [Fact] void should_not_replay_or_publish_from_the_old_connection() => values.ShouldContainOnly(3);
    [Fact] void should_not_dispose_the_new_connection_when_retiring_the_old_one() => liveReleasesDuringOldDisposal.ShouldEqual(0);
    [Fact] void should_release_both_connections_once() => sources.Select(source => source.releases).ShouldEqual([1, 1]);
}
