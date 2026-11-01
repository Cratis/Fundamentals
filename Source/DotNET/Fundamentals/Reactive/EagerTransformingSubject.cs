// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Cratis.Reactive;

/// <summary>Owns the two subscriptions of the eager, compatibility lifetime.</summary>
/// <typeparam name="TSource">Source value type.</typeparam>
/// <typeparam name="TResult">Result value type.</typeparam>
internal sealed class EagerTransformingSubject<TSource, TResult> : ISubject<TResult>, IDisposable
{
    readonly TransformingSubjectReplay<TResult> _result = new(new object());
    readonly SingleAssignmentDisposable _transformation = new();
    readonly SingleAssignmentDisposable _completion = new();
    int _disposed;

    /// <summary>Initializes a new instance of the <see cref="EagerTransformingSubject{TSource, TResult}"/> class.</summary>
    /// <param name="source">Caller-owned source facade.</param>
    /// <param name="transform">Transformation.</param>
    internal EagerTransformingSubject(ISubject<TSource> source, Func<TSource, TResult> transform)
    {
        try
        {
            // Keep the existing ordering: transformation first, completion monitor second.
            _transformation.Disposable = source.Select(transform).Subscribe(_result.PublishSource, _result.Fail, _result.Complete);
            _completion.Disposable = source.Subscribe(_ => { }, _ => { }, Dispose);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public void OnNext(TResult value) => _result.OnNext(value);

    /// <inheritdoc/>
    public void OnError(Exception error) => _result.OnError(error);

    /// <inheritdoc/>
    public void OnCompleted() => _result.OnCompleted();

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TResult> observer) => _result.Subscribe(observer);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        try
        {
            _transformation.Dispose();
        }
        finally
        {
            try
            {
                _completion.Dispose();
            }
            finally
            {
                _result.Dispose();
            }
        }
    }
}
