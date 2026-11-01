// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Cratis.Reactive.for_TransformingSubject.given;

public sealed class tracked_source : ISubject<int>, IDisposable
{
    readonly Subject<int> subject = new();
    public int subscriptions;
    public int releases;
    public int sourceDisposals;
    public Action<IObserver<int>> onSubscribe;
    public Action onRelease;

    public IDisposable Subscribe(IObserver<int> observer)
    {
        Interlocked.Increment(ref subscriptions);
        var inner = subject.Subscribe(observer);
        onSubscribe?.Invoke(observer);
        return Disposable.Create(() =>
        {
            inner.Dispose();
            Interlocked.Increment(ref releases);
            onRelease?.Invoke();
        });
    }

    public void OnNext(int value) => subject.OnNext(value);
    public void OnError(Exception error) => subject.OnError(error);
    public void OnCompleted() => subject.OnCompleted();
    public void Dispose()
    {
        sourceDisposals++;
        subject.Dispose();
    }
}
