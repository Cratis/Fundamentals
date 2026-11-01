// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;

namespace Cratis.Reactive;

/// <summary>Protects replay operations, terminal sealing and physical buffer disposal with operation leases.</summary>
/// <typeparam name="T">Result value type.</typeparam>
/// <param name="gate">Shared generation admission gate. Only bookkeeping runs under it.</param>
internal sealed class TransformingSubjectReplay<T>(object gate) : IDisposable
{
    readonly ReplaySubject<T> _result = new(1);
    List<TransformingSubjectSubscription<T>>? _pending;
    Exception? _error;
    int _operations;
    bool _closed;
    bool _sealing;
    bool _sealed;
    bool _disposed;
    bool _released;
    bool _retired;

    /// <summary>Optional subscription boundary instrumentation; invoked outside the gate with an operation lease.</summary>
    internal Action? BeforeSubscribe { get; set; }

    /// <summary>Gets whether owner disposal has cancelled attachment and delivery.</summary>
    internal bool IsDisposed => Volatile.Read(ref _disposed);

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (gate)
        {
            _disposed = true;
            _pending = null;
        }
        Drain();
    }

    /// <summary>Admits an entire transform/publish operation. The caller holds the shared gate.</summary>
    /// <returns>Whether the operation was admitted.</returns>
    internal bool TryEnter()
    {
        if (_closed || _disposed) return false;
        _operations++;
        return true;
    }

    /// <summary>Publishes while holding an operation lease, outside the gate.</summary>
    /// <param name="value">Result value.</param>
    internal void PublishAdmitted(T value) => _result.OnNext(value);

    /// <summary>Releases an operation lease and attempts deferred cleanup.</summary>
    internal void Exit()
    {
        lock (gate) _operations--;
        Drain();
    }

    /// <summary>Publishes an eager source value if admission is still open.</summary>
    /// <param name="value">Result value.</param>
    internal void PublishSource(T value)
    {
        lock (gate)
        {
            if (!TryEnter()) return;
        }
        try { PublishAdmitted(value); }
        finally { Exit(); }
    }

    /// <summary>Publishes a direct result, rejecting owner disposal.</summary>
    /// <param name="value">Result value.</param>
    internal void OnNext(T value)
    {
        lock (gate) ThrowIfDisposed();
        PublishSource(value);
    }

    /// <summary>Closes source admission with completion.</summary>
    internal void Complete() => Close(null);

    /// <summary>Closes source admission with an error.</summary>
    /// <param name="error">Terminal error.</param>
    internal void Fail(Exception error) => Close(error);

    /// <summary>Completes direct results, rejecting owner disposal.</summary>
    internal void OnCompleted()
    {
        lock (gate) ThrowIfDisposed();
        Complete();
    }

    /// <summary>Fails direct results, rejecting owner disposal.</summary>
    /// <param name="error">Terminal error.</param>
    internal void OnError(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        lock (gate) ThrowIfDisposed();
        Fail(error);
    }

    /// <summary>Closes admission under the shared gate without calling external code.</summary>
    /// <param name="error">Terminal error, or null for completion.</param>
    internal void Stop(Exception? error)
    {
        if (_closed || _disposed) return;
        _closed = true;
        _error = error;
    }

    /// <summary>Closes admission and attempts terminal delivery outside the gate.</summary>
    /// <param name="error">Terminal error, or null for completion.</param>
    internal void Close(Exception? error)
    {
        lock (gate) Stop(error);
        Drain();
    }

    /// <summary>Releases a retired generation's buffer after admitted operations and terminal sealing.</summary>
    internal void Retire()
    {
        lock (gate)
        {
            Stop(null);
            _retired = true;
        }
        Drain();
    }

    /// <summary>Reserves attachment, including a lease for already-sealed replay or a cancellable terminal-drain reservation.</summary>
    /// <param name="observer">Consumer.</param>
    /// <returns>The owned reservation.</returns>
    internal TransformingSubjectSubscription<T> Reserve(IObserver<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (gate)
        {
            ThrowIfDisposed();
            var subscription = new TransformingSubjectSubscription<T>(this, observer);
            if (_closed && !_sealed)
            {
                (_pending ??= []).Add(subscription);
            }
            else
            {
                _operations++;
                subscription.Ready = true;
            }
            return subscription;
        }
    }

    /// <summary>Reserves and attaches a consumer outside the gate.</summary>
    /// <param name="observer">Consumer.</param>
    /// <returns>The owned attachment.</returns>
    internal IDisposable Subscribe(IObserver<T> observer)
    {
        var subscription = Reserve(observer);
        subscription.Attach();
        return subscription;
    }

    /// <summary>Attaches to replay while the caller holds an operation lease.</summary>
    /// <param name="observer">Cancellable observer.</param>
    /// <returns>The replay subscription.</returns>
    internal IDisposable Attach(IObserver<T> observer)
    {
        BeforeSubscribe?.Invoke();
        return _result.Subscribe(observer);
    }

    /// <summary>Claims terminal sealing or physical disposal when operations have drained.</summary>
    internal void Drain()
    {
        bool dispose;
        lock (gate)
        {
            if (_operations != 0 || _released) return;
            dispose = _disposed || (_retired && _sealed);
            if (dispose) _disposed = true;
            if (dispose)
            {
                _released = true;
            }
            else if (_closed && !_sealed && !_sealing)
            {
                _sealing = true;
                _operations++;
            }
            else
            {
                return;
            }
        }
        if (dispose) _result.Dispose();
        else Seal();
    }

    static void AttachPending(List<TransformingSubjectSubscription<T>>? pending)
    {
        if (pending is null) return;
        List<Exception>? failures = null;
        foreach (var subscription in pending)
        {
            try { subscription.Attach(); }
            catch (Exception error) { (failures ??= []).Add(error); }
        }
        if (failures is not null) throw new AggregateException(failures);
    }

    [SuppressMessage("Usage", "CA1513:Use ObjectDisposedException throw helper", Justification = "Preserve ReplaySubject's existing exception message without an object name.")]
    void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(string.Empty);
    }

    void Seal()
    {
        try
        {
            if (_error is null) _result.OnCompleted();
            else _result.OnError(_error);
        }
        finally
        {
            List<TransformingSubjectSubscription<T>>? pending;
            lock (gate)
            {
                _sealing = false;
                _sealed = true;
                _operations--;
                pending = _pending;
                _pending = null;
                if (pending is not null)
                {
                    _operations += pending.Count;
                    foreach (var subscription in pending) subscription.Ready = true;
                }
            }
            try { AttachPending(pending); }
            finally { Drain(); }
        }
    }
}
