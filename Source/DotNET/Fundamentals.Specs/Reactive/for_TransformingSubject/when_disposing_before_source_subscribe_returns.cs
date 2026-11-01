// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_disposing_before_source_subscribe_returns : Specification
{
    given.tracked_source source;
    int transformations;
    int releasesBeforeReturn;

    async Task Because()
    {
        source = new();
        using var subscribed = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        source.onSubscribe = observer =>
        {
            subscribed.Set();
            if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            observer.OnNext(1);
            observer.OnCompleted();
        };
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value =>
        {
            transformations++;
            return value;
        });
        var connecting = Task.Run(() => subject.Subscribe(_ => { }));
        try
        {
            if (!subscribed.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            subject.Dispose();
            subject.Dispose();
            releasesBeforeReturn = source.releases;
        }
        finally { resume.Set(); }
        using var consumer = await connecting.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact] void should_not_require_a_connection_handle_to_return_from_dispose() => releasesBeforeReturn.ShouldEqual(0);
    [Fact] void should_dispose_the_late_assigned_handle_once() => source.releases.ShouldEqual(1);
    [Fact] void should_ignore_late_source_callbacks() => transformations.ShouldEqual(0);
}
