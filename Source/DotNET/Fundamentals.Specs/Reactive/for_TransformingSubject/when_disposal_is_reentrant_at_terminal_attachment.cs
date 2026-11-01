// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_disposal_is_reentrant_at_terminal_attachment : Specification
{
    List<int> received;
    int completions;
    int boundaryCalls;
    Exception failure;

    void Because()
    {
        received = [];
        foreach (var pendingTerminal in new[] { false, true })
        {
            var gate = new object();
            using var replay = new TransformingSubjectReplay<int>(gate);
            replay.OnNext(1);
            if (pendingTerminal)
            {
                lock (gate)
                {
                    replay.TryEnter().ShouldBeTrue();
                    replay.Stop(null);
                }
            }
            else
            {
                replay.OnCompleted();
            }
            replay.BeforeSubscribe = () =>
            {
                boundaryCalls++;
                replay.Dispose();
            };
            try
            {
                using var subscription = replay.Subscribe(System.Reactive.Observer.Create<int>(received.Add, () => completions++));
                if (pendingTerminal) replay.Exit();
            }
            catch (Exception error) { failure = error; }
        }
    }

    [Fact] void should_reach_both_attachment_boundaries() => boundaryCalls.ShouldEqual(2);
    [Fact] void should_not_wait_for_or_dispose_its_own_operation() => failure.ShouldBeNull();
    [Fact] void should_suppress_replay_after_reentrant_disposal() => received.ShouldBeEmpty();
    [Fact] void should_suppress_terminal_delivery_after_reentrant_disposal() => completions.ShouldEqual(0);
}
