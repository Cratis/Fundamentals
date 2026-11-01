// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_TransformingSubject;

public class when_publishing_a_result_directly : Specification
{
    List<int> received;
    int transformed;
    int sourceValues;

    void Because()
    {
        using var source = new Subject<int>();
        using var independent = source.Subscribe(_ => sourceValues++);
        using var subject = new TransformingSubject<int, int>(source, value =>
        {
            transformed++;
            return value * 2;
        });
        subject.OnNext(7);
        received = [];
        using var consumer = subject.Subscribe(received.Add);
    }

    [Fact] void should_replay_the_untransformed_result() => received.ShouldContainOnly(7);
    [Fact] void should_not_transform_a_result() => transformed.ShouldEqual(0);
    [Fact] void should_not_publish_to_the_source() => sourceValues.ShouldEqual(0);
}
