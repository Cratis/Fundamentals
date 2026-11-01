// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;

namespace Cratis.Reactive;

/// <summary>A cancellable, assignment-safe replay attachment, including terminal-drain attachment.</summary>
/// <typeparam name="T">Result value type.</typeparam>
/// <param name="replay">Replay lifetime.</param>
/// <param name="observer">Consumer.</param>
internal sealed class TransformingSubjectSubscription<T>(TransformingSubjectReplay<T> replay, IObserver<T> observer) : IObserver<T>, IDisposable
{
    /// <summary>Whether an attachment operation lease has been reserved.</summary>
    internal bool Ready;

    readonly SingleAssignmentDisposable _subscription = new();
    int _cancelled;
    int _attached;

    bool IsCancelled => Volatile.Read(ref _cancelled) != 0 || replay.IsDisposed;

    /// <inheritdoc/>
    public void OnNext(T value)
    {
        if (!IsCancelled) observer.OnNext(value);
    }

    /// <inheritdoc/>
    public void OnError(Exception error)
    {
        if (!IsCancelled) observer.OnError(error);
    }

    /// <inheritdoc/>
    public void OnCompleted()
    {
        if (!IsCancelled) observer.OnCompleted();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Interlocked.Exchange(ref _cancelled, 1);
        _subscription.Dispose();
    }

    /// <summary>Delivers replay outside the gate while holding the reserved operation lease.</summary>
    internal void Attach()
    {
        if (!Volatile.Read(ref Ready) || Interlocked.Exchange(ref _attached, 1) != 0) return;
        try
        {
            if (!IsCancelled) _subscription.Disposable = replay.Attach(this);
        }
        catch
        {
            Dispose();
            throw;
        }
        finally
        {
            replay.Exit();
        }
    }
}
