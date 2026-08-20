// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_ObservableExtensions;

public class when_wrapping_a_source_that_emits_synchronously_on_subscribe : Specification
{
    BehaviorSubject<string> source;
    ISubject<string> result;
    List<string> received;

    void Establish()
    {
        source = new("initial");
        received = [];
    }

    void Because()
    {
        result = this.InvokeAndWrapWithSubject<string>(_ => source);
        result.Subscribe(received.Add);
    }

    [Fact] void should_deliver_the_value_the_source_emitted_before_anyone_subscribed() => received.ShouldContainOnly("initial");
}
