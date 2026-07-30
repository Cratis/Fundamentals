// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_ObservableExtensions;

public class when_the_wrapping_subject_is_completed : Specification
{
    Subject<string> source;
    ISubject<string> result;
    CancellationToken tokenPassedToTheAction;
    bool sourceStillHasObservers;

    void Establish()
    {
        source = new();
        result = this.InvokeAndWrapWithSubject<string>(token =>
        {
            tokenPassedToTheAction = token;
            return source;
        });
    }

    void Because()
    {
        result.OnCompleted();
        sourceStillHasObservers = source.HasObservers;
    }

    [Fact] void should_cancel_the_token_handed_to_the_action() => tokenPassedToTheAction.IsCancellationRequested.ShouldBeTrue();

    [Fact] void should_unsubscribe_from_the_source() => sourceStillHasObservers.ShouldBeFalse();
}
