// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_disposing_during_a_sealed_late_attachment : Specification
{
    List<int> received;
    int completions;
    Exception failure;

    async Task Because()
    {
        using var replay = new TransformingSubjectReplay<int>(new object());
        replay.OnNext(1);
        replay.OnCompleted();
        using var chosen = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        replay.BeforeSubscribe = () =>
        {
            chosen.Set();
            if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
        };
        received = [];
        var attachment = Task.Run(() =>
        {
            try
            {
                using var subscription = replay.Subscribe(System.Reactive.Observer.Create<int>(received.Add, () => completions++));
            }
            catch (Exception error) { failure = error; }
        });
        try
        {
            if (!chosen.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            replay.Dispose();
        }
        finally { resume.Set(); }
        await attachment.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact] void should_not_access_a_physically_disposed_replay() => failure.ShouldBeNull();
    [Fact] void should_cancel_the_late_replay_value() => received.ShouldBeEmpty();
    [Fact] void should_cancel_the_late_terminal_delivery() => completions.ShouldEqual(0);
}
