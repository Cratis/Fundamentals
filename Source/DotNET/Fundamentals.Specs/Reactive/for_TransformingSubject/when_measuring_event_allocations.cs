// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive.for_TransformingSubject;

public class when_measuring_event_allocations : Specification
{
    long allocated;

    void Because()
    {
        using var source = new Subject<int>();
        using var subject = new TransformingSubject<int, int>(source, value => value);
        using var subscription = subject.Subscribe(new CountingObserver());
        for (var sample = 0; sample < 4; sample++)
        {
            for (var index = 0; index < 10000; index++) source.OnNext(index);
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 100000; index++) source.OnNext(index);
            allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Console.WriteLine($"allocation sample={sample} bytes={allocated} events=100000 runtime={Environment.Version}");
        }
    }

    [Fact] void should_not_allocate_per_source_event() => allocated.ShouldEqual(0L);

    sealed class CountingObserver : IObserver<int>
    {
        public void OnNext(int value) { }
        public void OnError(Exception error) { }
        public void OnCompleted() { }
    }
}
