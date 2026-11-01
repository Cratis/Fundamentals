// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_retiring_during_an_admitted_transform : Specification
{
    given.tracked_source[] sources;
    List<int> values;
    int factories;
    int oldReleasesBeforeDrain;

    async Task Because()
    {
        sources = [new(), new()];
        using var entered = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(
        () => sources[factories++],
        value =>
        {
            if (value == 1)
            {
                entered.Set();
                if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            }
            return value * 2;
        });
        var first = subject.Subscribe(_ => { });
        var publishing = Task.Run(() => sources[0].OnNext(1));
        values = [];
        try
        {
            if (!entered.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            first.Dispose();
            oldReleasesBeforeDrain = sources[0].releases;
            using var next = subject.Subscribe(values.Add);
            sources[1].OnNext(2);
        }
        finally { resume.Set(); }
        await publishing.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact] void should_disconnect_without_waiting_for_the_old_transform() => oldReleasesBeforeDrain.ShouldEqual(1);
    [Fact] void should_create_a_fresh_generation_without_waiting_for_the_old_transform() => factories.ShouldEqual(2);
    [Fact] void should_not_publish_the_old_result_into_the_new_buffer() => values.ShouldContainOnly(4);
    [Fact] void should_release_each_connection_once() => sources.Select(source => source.releases).ShouldEqual([1, 1]);
}
