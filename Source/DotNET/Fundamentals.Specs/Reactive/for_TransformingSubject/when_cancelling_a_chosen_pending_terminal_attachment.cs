// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_cancelling_a_chosen_pending_terminal_attachment : Specification
{
    List<int> received;
    int completions;
    Exception failure;

    async Task Because()
    {
        var gate = new object();
        using var replay = new TransformingSubjectReplay<int>(gate);
        replay.OnNext(1);
        lock (gate)
        {
            replay.TryEnter().ShouldBeTrue();
            replay.Stop(null);
        }
        received = [];
        using var pending = replay.Subscribe(System.Reactive.Observer.Create<int>(received.Add, () => completions++));
        using var chosen = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        replay.BeforeSubscribe = () =>
        {
            chosen.Set();
            if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
        };
        var draining = Task.Run(() =>
        {
            try { replay.Exit(); }
            catch (Exception error) { failure = error; }
        });
        try
        {
            if (!chosen.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            pending.Dispose();
            replay.Dispose();
        }
        finally { resume.Set(); }
        await draining.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact] void should_not_access_a_physically_disposed_replay() => failure.ShouldBeNull();
    [Fact] void should_cancel_the_pending_replay_value() => received.ShouldBeEmpty();
    [Fact] void should_cancel_the_pending_completion() => completions.ShouldEqual(0);
}
