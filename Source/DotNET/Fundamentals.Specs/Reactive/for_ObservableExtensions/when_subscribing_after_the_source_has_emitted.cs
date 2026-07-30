// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_ObservableExtensions;

public class when_subscribing_after_the_source_has_emitted : Specification
{
    Subject<string> source;
    ISubject<string> result;
    List<string> received;

    void Establish()
    {
        source = new();
        received = [];
        result = this.InvokeAndWrapWithSubject<string>(_ => source);
        source.OnNext("emitted before anyone subscribed");
    }

    void Because() => result.Subscribe(received.Add);

    [Fact] void should_replay_the_most_recent_value_to_the_late_subscriber() => received.ShouldContainOnly("emitted before anyone subscribed");
}
