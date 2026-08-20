// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Cratis.Reactive;

/// <summary>
/// Represents a subject that transforms the input before passing it on to the observers.
/// </summary>
/// <typeparam name="TSource">Type of source.</typeparam>
/// <typeparam name="TResult">Type of converted result.</typeparam>
/// <remarks>
/// The transformed result replays its most recent value to whoever subscribes. The source is subscribed to during
/// construction, before any consumer can have attached, so a source emitting synchronously on subscribe - a
/// <see cref="BehaviorSubject{T}"/> seed, a <see cref="ReplaySubject{T}"/> buffer - would otherwise have that first
/// value dropped on the floor.
/// </remarks>
public class TransformingSubject<TSource, TResult> : ISubject<TResult>, IDisposable
{
    readonly ISubject<TSource> _sourceSubject;
    readonly ReplaySubject<TResult> _resultSubject;

    /// <summary>
    /// Initializes a new instance of <see cref="TransformingSubject{TSource, TResult}"/>.
    /// </summary>
    /// <param name="sourceSubject">Subject to observe.</param>
    /// <param name="transform">Callback for doing transformation.</param>
    public TransformingSubject(ISubject<TSource> sourceSubject, Func<TSource, TResult> transform)
    {
        _sourceSubject = sourceSubject;
        _resultSubject = new ReplaySubject<TResult>(1);
        _sourceSubject.Select(transform).Subscribe(_resultSubject);
        sourceSubject.Subscribe(_ => { }, _ => { }, Dispose);
    }

    /// <inheritdoc/>
    public void OnNext(TResult value) => _resultSubject.OnNext(value);

    /// <inheritdoc/>
    public void OnError(Exception error) => _sourceSubject.OnError(error);

    /// <inheritdoc/>
    public void OnCompleted() => _sourceSubject.OnCompleted();

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TResult> observer) => _resultSubject.Subscribe(observer);

    /// <inheritdoc/>
    public void Dispose()
    {
        _resultSubject.Dispose();
    }
}
