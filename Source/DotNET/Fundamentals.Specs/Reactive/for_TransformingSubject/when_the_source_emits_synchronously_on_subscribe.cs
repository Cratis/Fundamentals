// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Reactive.Subjects;

namespace Cratis.Reactive.for_TransformingSubject;

public class when_the_source_emits_synchronously_on_subscribe : Specification
{
    BehaviorSubject<int> source;
    TransformingSubject<int, string> subject;
    List<string> received;

    void Establish()
    {
        source = new(42);
        received = [];
    }

    void Because()
    {
        subject = new(source, value => value.ToString(CultureInfo.InvariantCulture));
        subject.Subscribe(received.Add);
    }

    [Fact] void should_deliver_the_value_the_source_emitted_during_construction() => received.ShouldContainOnly("42");
}
