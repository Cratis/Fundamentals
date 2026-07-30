// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_ObservableExtensions;

public class when_transforming_a_source_that_emits_synchronously_on_subscribe : Specification
{
    BehaviorSubject<IEnumerable<string>> source;
    ISubject<IEnumerable<string>> result;
    List<IEnumerable<string>> received;

    void Establish()
    {
        source = new(["first", "second"]);
        received = [];
    }

    void Because()
    {
        result = this.InvokeAndWrapWithTransformSubject<IEnumerable<string>, IEnumerable<string>>(
            _ => source,
            values => values.Select(value => value.ToUpperInvariant()));
        result.Subscribe(received.Add);
    }

    [Fact] void should_deliver_one_value() => received.Count.ShouldEqual(1);

    [Fact] void should_transform_the_value_the_source_emitted_before_anyone_subscribed() => received[0].ShouldContainOnly("FIRST", "SECOND");
}
