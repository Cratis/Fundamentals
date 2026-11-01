// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_reusing_a_genuinely_cold_facade : Specification
{
    int connections;
    int releases;
    int transformations;
    List<int> values;

    void Because()
    {
        var source = given.cold_facade.reusable(() => connections++, () => releases++);
        using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value =>
        {
            transformations++;
            return value * 2;
        });
        values = [];
        using (subject.Subscribe(values.Add)) { }
        using (subject.Subscribe(values.Add)) { }
    }

    [Fact] void should_start_an_independent_connection_for_each_generation() => connections.ShouldEqual(2);
    [Fact] void should_release_each_independent_connection() => releases.ShouldEqual(2);
    [Fact] void should_transform_each_connection_value_once() => transformations.ShouldEqual(2);
    [Fact] void should_deliver_each_synchronous_connection_value_without_stale_replay() => values.ShouldEqual([2, 2]);
}
