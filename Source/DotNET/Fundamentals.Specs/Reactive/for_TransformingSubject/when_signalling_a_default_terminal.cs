// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_signalling_a_default_terminal : Specification
{
    List<string> order;
    List<Exception> sourceErrors;
    List<Exception> resultErrors;
    int sourceCompletions;
    int resultCompletions;
    readonly Exception expected = new("forwarded error");

    void Because()
    {
        order = [];
        sourceErrors = [];
        resultErrors = [];
        foreach (var complete in new[] { true, false })
        {
            foreach (var alreadyDisposed in new[] { false, true })
            {
                var source = new given.tracked_source();
                using var independent = source.Subscribe(
                _ => { },
                error =>
                {
                    sourceErrors.Add(error);
                    order.Add("source error");
                },
                () =>
                {
                    sourceCompletions++;
                    order.Add("source complete");
                });
                using var subject = new TransformingSubject<int, int>(source, value => value);
                using var consumer = subject.Subscribe(
                _ => { },
                error =>
                {
                    resultErrors.Add(error);
                    order.Add("result error");
                },
                () =>
                {
                    resultCompletions++;
                    order.Add("result complete");
                });
                if (alreadyDisposed) subject.Dispose();
                if (complete) subject.OnCompleted();
                else subject.OnError(expected);
            }
        }
    }

    [Fact] void should_forward_completion_even_after_owner_disposal() => sourceCompletions.ShouldEqual(2);
    [Fact] void should_forward_the_same_error_even_after_owner_disposal() => sourceErrors.ShouldEqual([expected, expected]);
    [Fact] void should_complete_the_live_result_through_the_source() => resultCompletions.ShouldEqual(1);
    [Fact] void should_fail_the_live_result_with_the_forwarded_error() => resultErrors.ShouldContainOnly(expected);
    [Fact] void should_reach_the_source_observers_before_the_result_observers() => order.ShouldEqual(["source complete", "result complete", "source complete", "source error", "result error", "source error"]);
}
