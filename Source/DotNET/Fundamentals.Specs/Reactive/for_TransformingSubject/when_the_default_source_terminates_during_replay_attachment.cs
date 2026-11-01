// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_the_default_source_terminates_during_replay_attachment : Specification
{
    List<int> activeTerminals;
    List<int> attachmentTerminalsBeforeResume;
    List<int> attachmentTerminalsAfterResume;
    List<Exception> errors;
    List<Exception?> lateFailures;
    readonly Exception expected = new("source error");

    async Task Because()
    {
        activeTerminals = [];
        attachmentTerminalsBeforeResume = [];
        attachmentTerminalsAfterResume = [];
        errors = [];
        lateFailures = [];
        foreach (var complete in new[] { true, false })
        {
            var source = new given.tracked_source();
            using var subject = new TransformingSubject<int, int>(source, value => value * 2);
            var activeCount = 0;
            var attachmentCount = 0;
            using var first = subject.Subscribe(_ => { }, _ => activeCount++, () => activeCount++);
            using var second = subject.Subscribe(_ => { }, _ => activeCount++, () => activeCount++);
            source.OnNext(1);
            using var replaying = new ManualResetEventSlim();
            using var resume = new ManualResetEventSlim();
            var attaching = Task.Run(() => subject.Subscribe(
            _ =>
            {
                replaying.Set();
                if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            },
            error =>
            {
                errors.Add(error);
                attachmentCount++;
            },
            () => attachmentCount++));
            try
            {
                if (!replaying.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
                var terminating = Task.Run(() =>
                {
                    if (complete) source.OnCompleted();
                    else source.OnError(expected);
                });
                await terminating.WaitAsync(TimeSpan.FromSeconds(10));
                activeTerminals.Add(activeCount);
                attachmentTerminalsBeforeResume.Add(attachmentCount);
                lateFailures.Add(Catch.Exception(() =>
                {
                    using var late = subject.Subscribe(_ => { }, _ => { });
                }));
            }
            finally { resume.Set(); }
            using var attachment = await attaching.WaitAsync(TimeSpan.FromSeconds(10));
            attachmentTerminalsAfterResume.Add(attachmentCount);
        }
    }

    [Fact] void should_deliver_the_terminal_to_all_current_subscribers_before_returning() => activeTerminals.ShouldEqual([2, 2]);
    [Fact] void should_not_interrupt_a_replay_callback_to_deliver_a_terminal() => attachmentTerminalsBeforeResume.ShouldEqual([0, 0]);
    [Fact] void should_preserve_queued_terminals_for_the_in_flight_attachment() => attachmentTerminalsAfterResume.ShouldEqual([1, 1]);
    [Fact] void should_preserve_the_source_error_for_the_attachment() => errors.ShouldContainOnly(expected);
    [Fact] void should_preserve_disposed_late_subscription_after_source_completion() => lateFailures[0].ShouldBeOfExactType<ObjectDisposedException>();
    [Fact] void should_preserve_late_subscription_after_source_error() => lateFailures[1].ShouldBeNull();
}
