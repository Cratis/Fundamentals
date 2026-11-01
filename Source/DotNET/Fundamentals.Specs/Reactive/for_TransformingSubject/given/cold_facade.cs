// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Cratis.Reactive.for_TransformingSubject.given;

public sealed class cold_facade(Func<IObserver<int>, IDisposable> subscribe) : ISubject<int>
{
    public static cold_facade reusable(Action connected, Action released) => new(observer =>
    {
        connected();
        observer.OnNext(1);
        return Disposable.Create(released);
    });

    public IDisposable Subscribe(IObserver<int> observer) => subscribe(observer);
    public void OnNext(int value) { }
    public void OnError(Exception error) { }
    public void OnCompleted() { }
}
