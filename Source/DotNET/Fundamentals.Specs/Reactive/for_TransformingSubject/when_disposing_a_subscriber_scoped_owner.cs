// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_disposing_a_subscriber_scoped_owner : Specification
{
    given.tracked_source source;
    int independentValues;
    int transformations;
    Exception lateFailure;
    Exception nextFailure;
    Exception errorFailure;
    Exception completionFailure;

    void Because()
    {
        source = new();
        using var independent = source.Subscribe(_ => independentValues++);
        var subject = TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value =>
        {
            transformations++;
            return value;
        });
        using var consumer = subject.Subscribe(_ => { });
        subject.Dispose();
        subject.Dispose();
        source.OnNext(1);
        lateFailure = Catch.Exception(() => subject.Subscribe(_ => { }));
        nextFailure = Catch.Exception(() => subject.OnNext(1));
        errorFailure = Catch.Exception(() => subject.OnError(new Exception()));
        completionFailure = Catch.Exception(subject.OnCompleted);
    }

    [Fact] void should_release_only_owned_handles_until_independent_disposal() => source.releases.ShouldEqual(2);
    [Fact] void should_not_dispose_the_source_facade() => source.sourceDisposals.ShouldEqual(0);
    [Fact] void should_leave_an_independent_observer_alive() => independentValues.ShouldEqual(1);
    [Fact] void should_not_transform_after_disposal() => transformations.ShouldEqual(0);
    [Fact] void should_reject_late_subscriptions() => lateFailure.ShouldBeOfExactType<ObjectDisposedException>();
    [Fact] void should_reject_late_values() => nextFailure.ShouldBeOfExactType<ObjectDisposedException>();
    [Fact] void should_reject_late_errors() => errorFailure.ShouldBeOfExactType<ObjectDisposedException>();
    [Fact] void should_reject_late_completion() => completionFailure.ShouldBeOfExactType<ObjectDisposedException>();
}
