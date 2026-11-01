// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_TransformingSubject;

public class when_connection_setup_fails : Specification
{
    List<Exception> errors;
    List<Exception> lateErrors;
    int factories;
    readonly Exception expected = new("setup failed");

    void Because()
    {
        errors = [];
        lateErrors = [];
        Func<ISubject<int>>[] failures =
        [
            () => throw expected,
            () => null!,
            () => new given.cold_facade(_ => throw expected)
        ];
        foreach (var factory in failures)
        {
            using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(
            () =>
            {
                factories++;
                return factory();
            },
            value => value);
            using var current = subject.Subscribe(_ => { }, errors.Add);
            using var late = subject.Subscribe(_ => { }, lateErrors.Add);
        }
    }

    [Fact] void should_not_retry_any_failed_setup() => factories.ShouldEqual(3);
    [Fact] void should_deliver_each_setup_failure() => errors.Count.ShouldEqual(3);
    [Fact] void should_retain_each_setup_failure_for_late_consumers() => lateErrors.ShouldEqual(errors);
    [Fact] void should_preserve_the_factory_exception() => errors[0].ShouldBeSame(expected);
    [Fact] void should_reject_a_null_factory_result() => errors[1].ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_preserve_the_subscribe_exception() => errors[2].ShouldBeSame(expected);
}
