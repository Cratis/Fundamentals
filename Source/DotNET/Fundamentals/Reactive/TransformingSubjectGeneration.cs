// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;

namespace Cratis.Reactive;

/// <summary>Owns one independent connection and its replay buffer.</summary>
/// <typeparam name="TSource">Source value type.</typeparam>
/// <typeparam name="TResult">Result value type.</typeparam>
/// <param name="owner">Generation owner.</param>
/// <param name="gate">Shared admission gate.</param>
internal sealed class TransformingSubjectGeneration<TSource, TResult>(SubscriberScopedTransformingSubject<TSource, TResult> owner, object gate) : IObserver<TSource>, IDisposable
{
    /// <summary>Assignment-safe connection, available before external Subscribe.</summary>
    internal readonly SingleAssignmentDisposable Connection = new();

    /// <summary>Generation-local replay lifetime.</summary>
    internal readonly TransformingSubjectReplay<TResult> Replay = new(gate);

    /// <summary>Reservations, guarded by the owner's gate.</summary>
    internal int Consumers;

    /// <summary>Whether admission has ended, guarded by the owner's gate.</summary>
    internal bool Closed;

    /// <inheritdoc/>
    public void OnNext(TSource value) => owner.Publish(this, value);

    /// <inheritdoc/>
    public void OnError(Exception error) => owner.Terminate(this, error, false);

    /// <inheritdoc/>
    public void OnCompleted() => owner.Terminate(this, null, false);

    /// <inheritdoc/>
    public void Dispose()
    {
        try { Connection.Dispose(); }
        finally { Replay.Dispose(); }
    }
}
