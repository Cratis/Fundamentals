// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;

namespace Cratis.Reactive.for_TransformingSubject;

public class when_a_source_terminates_synchronously_during_subscribe : Specification
{
    List<int> releases;
    List<int> values;
    int completions;
    int failures;

    void Because()
    {
        releases = [];
        values = [];
        foreach (var mode in new[] { "complete", "error", "transform" })
        {
            var released = 0;
            var source = new given.cold_facade(observer =>
            {
                observer.OnNext(1);
                if (mode == "error") observer.OnError(new Exception("source failed"));
                else observer.OnCompleted();
                return Disposable.Create(() => released++);
            });
            using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value => mode == "transform" ? throw new Exception("transform failed") : value * 2);
            using var consumer = subject.Subscribe(values.Add, _ => failures++, () => completions++);
            subject.Dispose();
            subject.Dispose();
            releases.Add(released);
        }
    }

    [Fact] void should_release_every_late_assigned_connection_exactly_once() => releases.ShouldEqual([1, 1, 1]);
    [Fact] void should_deliver_each_successfully_transformed_synchronous_value() => values.ShouldEqual([2, 2]);
    [Fact] void should_deliver_synchronous_completion() => completions.ShouldEqual(1);
    [Fact] void should_deliver_synchronous_source_and_transform_errors() => failures.ShouldEqual(2);
}
