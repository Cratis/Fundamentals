// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_TransformingSubject;

public class when_capturing_values_across_a_consumer_gap : Specification
{
    List<int> received;
    int transformations;

    void Because()
    {
        using var source = new BehaviorSubject<int>(1);
        using var subject = new TransformingSubject<int, int>(source, value =>
        {
            transformations++;
            return value * 2;
        });
        using (subject.Subscribe(_ => { })) { }
        source.OnNext(3);
        received = [];
        using var subscription = subject.Subscribe(received.Add);
    }

    [Fact] void should_capture_the_new_value_without_a_consumer() => received.ShouldContainOnly(6);
    [Fact] void should_transform_during_construction_and_the_gap() => transformations.ShouldEqual(2);
}
