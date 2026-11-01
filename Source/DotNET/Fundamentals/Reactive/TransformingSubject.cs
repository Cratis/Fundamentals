// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Reactive;

/// <summary>
/// Represents an <see cref="ISubject{T}"/> that transforms the source observable to a different type.
/// </summary>
/// <typeparam name="TSource">Type of source.</typeparam>
/// <typeparam name="TResult">Type of result.</typeparam>
public class TransformingSubject<TSource, TResult> : ISubject<TResult>, IDisposable
{
    readonly ISubject<TResult> _subject;
    readonly IDisposable _lifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformingSubject{TSource, TResult}"/> class.
    /// </summary>
    /// <param name="sourceSubject">Source subject.</param>
    /// <param name="transform">Transform function.</param>
    public TransformingSubject(ISubject<TSource> sourceSubject, Func<TSource, TResult> transform)
        : this(new EagerTransformingSubject<TSource, TResult>(sourceSubject, transform))
    {
    }

    TransformingSubject(EagerTransformingSubject<TSource, TResult> subject)
    {
        _subject = subject;
        _lifetime = subject;
    }

    TransformingSubject(SubscriberScopedTransformingSubject<TSource, TResult> subject)
    {
        _subject = subject;
        _lifetime = subject;
    }

    /// <summary>
    /// Creates a lazy, subscriber-scoped subject for a verified cold, restartable source.
    /// </summary>
    /// <param name="sourceFactory">Factory whose subscriptions own independent connection resources. A reusable cold facade is allowed; an eager, one-shot subject is not.</param>
    /// <param name="transform">Transform function.</param>
    /// <returns>A subject that connects on its first subscriber and disconnects on its last subscriber.</returns>
    /// <remarks>
    /// Concurrent subscribers share one connection and replay one transformed value. After the last
    /// subscriber leaves, a new subscriber starts a fresh generation without stale replay. Values sent
    /// directly while idle are dropped. Completion and errors are permanent and retained for late
    /// subscribers. Disposal releases subscriptions, never the factory's subject. Callbacks already
    /// admitted may finish; reentrant cleanup is deferred until those callbacks unwind, without waiting.
    /// Existing eager subjects must continue using the constructor, not this factory.
    /// </remarks>
    public static TransformingSubject<TSource, TResult> CreateSubscriberScoped(Func<ISubject<TSource>> sourceFactory, Func<TSource, TResult> transform)
    {
        ArgumentNullException.ThrowIfNull(sourceFactory);
        ArgumentNullException.ThrowIfNull(transform);
        return new(new SubscriberScopedTransformingSubject<TSource, TResult>(sourceFactory, transform));
    }

    /// <inheritdoc/>
    public void Dispose() => _lifetime.Dispose();

    /// <inheritdoc/>
    public void OnCompleted() => _subject.OnCompleted();

    /// <inheritdoc/>
    public void OnError(Exception error) => _subject.OnError(error);

    /// <inheritdoc/>
    public void OnNext(TResult value) => _subject.OnNext(value);

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TResult> observer) => _subject.Subscribe(observer);
}
