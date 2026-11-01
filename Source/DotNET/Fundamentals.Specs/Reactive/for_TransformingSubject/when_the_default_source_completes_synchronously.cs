// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;

namespace Cratis.Reactive.for_TransformingSubject;

public class when_the_default_source_completes_synchronously : Specification
{
    int subscriptions;
    int releases;
    Exception lateFailure;

    void Because()
    {
        var source = new given.cold_facade(observer =>
        {
            subscriptions++;
            observer.OnNext(1);
            observer.OnCompleted();
            return Disposable.Create(() => releases++);
        });
        using var subject = new TransformingSubject<int, int>(source, value => value * 2);
        lateFailure = Catch.Exception(() => subject.Subscribe(_ => { }));
    }

    [Fact] void should_preserve_the_two_subscription_default() => subscriptions.ShouldEqual(2);
    [Fact] void should_release_both_late_assigned_handles() => releases.ShouldEqual(2);
    [Fact] void should_preserve_the_disposed_late_subscription_behavior() => lateFailure.ShouldBeOfExactType<ObjectDisposedException>();
}
