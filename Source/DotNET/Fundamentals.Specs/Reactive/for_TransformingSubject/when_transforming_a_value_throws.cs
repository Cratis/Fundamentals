// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_transforming_a_value_throws : Specification
{
    given.tracked_source source;
    readonly Exception expected = new("transform failed");
    Exception currentError;
    Exception lateError;
    List<int> lateValues;
    int factories;
    int transformations;

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
            return value == 1 ? 2 : throw expected;
        });
        using var current = subject.Subscribe(_ => { }, error => currentError = error);
        source.OnNext(1);
        source.OnNext(2);
        source.OnNext(3);
        subject.OnCompleted();
        lateValues = [];
        using var late = subject.Subscribe(lateValues.Add, error => lateError = error);
    }

    [Fact] void should_deliver_the_original_error_to_the_current_consumer() => currentError.ShouldBeSame(expected);
    [Fact] void should_retain_the_original_error_for_a_late_consumer() => lateError.ShouldBeSame(expected);
    [Fact] void should_retain_the_last_successful_result() => lateValues.ShouldContainOnly(2);
    [Fact] void should_stop_transforming_after_failure() => transformations.ShouldEqual(2);
    [Fact] void should_not_reconnect_after_failure() => factories.ShouldEqual(1);
    [Fact] void should_release_the_connection_on_failure() => source.releases.ShouldEqual(1);
}
