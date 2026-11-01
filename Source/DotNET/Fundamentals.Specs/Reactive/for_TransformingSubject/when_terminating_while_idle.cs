// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_terminating_while_idle : Specification
{
    int factories;
    int completions;
    List<Exception> errors;
    List<int> values;
    readonly Exception expected = new("terminal error");

    void Because()
    {
        errors = [];
        values = [];
        foreach (var fail in new[] { false, true })
        {
            using var subject = TransformingSubject<int, int>.CreateSubscriberScoped(
            () =>
            {
                factories++;
                return new given.tracked_source();
            },
            value => value);
            subject.OnNext(1);
            if (fail) subject.OnError(expected);
            else subject.OnCompleted();
            subject.OnCompleted();
            subject.OnError(new Exception());
            using var late = subject.Subscribe(values.Add, errors.Add, () => completions++);
        }
    }

    [Fact] void should_not_connect_for_an_idle_terminal_signal_or_late_subscriber() => factories.ShouldEqual(0);
    [Fact] void should_not_retain_idle_values() => values.ShouldBeEmpty();
    [Fact] void should_preserve_completion_as_the_first_terminal_winner() => completions.ShouldEqual(1);
    [Fact] void should_preserve_error_as_the_first_terminal_winner() => errors.ShouldContainOnly(expected);
}
