// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;

namespace Cratis.Reactive;

/// <summary>Admits operations and isolates independent subscriber-scoped generations.</summary>
/// <typeparam name="TSource">Source value type.</typeparam>
/// <typeparam name="TResult">Result value type.</typeparam>
/// <param name="sourceFactory">Verified cold, restartable source factory.</param>
/// <param name="transform">Transformation.</param>
internal sealed class SubscriberScopedTransformingSubject<TSource, TResult>(Func<ISubject<TSource>> sourceFactory, Func<TSource, TResult> transform) : ISubject<TResult>, IDisposable
{
    readonly object _gate = new();
    TransformingSubjectGeneration<TSource, TResult>? _current;
    bool _terminal;
    bool _disposed;

    /// <inheritdoc/>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reserve transfers its attachment to the consumer, which is disposed if attachment or connection setup throws.")]
    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        TransformingSubjectConsumer<TSource, TResult> consumer;
        TransformingSubjectGeneration<TSource, TResult> generation;
        bool connect;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            connect = _current is null && !_terminal;
            generation = _current ??= new(this, _gate);
            var attachment = generation.Replay.Reserve(observer);
            if (!_terminal) generation.Consumers++;
            consumer = new(this, generation, attachment, !_terminal);
        }
        try
        {
            consumer.Attach();
            if (connect) Connect(generation);
            return consumer;
        }
        catch
        {
            consumer.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public void OnNext(TResult value)
    {
        TransformingSubjectGeneration<TSource, TResult>? generation;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            generation = _current;
            if (_terminal || generation?.Replay.TryEnter() != true) return;
        }
        try { generation.Replay.PublishAdmitted(value); }
        finally { generation.Replay.Exit(); }
    }

    /// <inheritdoc/>
    public void OnError(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Terminate(null, error, true);
    }

    /// <inheritdoc/>
    public void OnCompleted() => Terminate(null, null, true);

    /// <inheritdoc/>
    public void Dispose()
    {
        TransformingSubjectGeneration<TSource, TResult>? generation;
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            generation = _current;
            _current = null;
            generation?.Closed = true;
        }
        generation?.Dispose();
    }

    /// <summary>Transforms and publishes one admitted source value without holding the gate.</summary>
    /// <param name="generation">Originating generation.</param>
    /// <param name="value">Source value.</param>
    internal void Publish(TransformingSubjectGeneration<TSource, TResult> generation, TSource value)
    {
        lock (_gate)
        {
            if (_disposed || _terminal || generation.Closed || !generation.Replay.TryEnter()) return;
        }
        try
        {
            TResult result;
            try { result = transform(value); }
            catch (Exception error)
            {
                Terminate(generation, error, false);
                return;
            }
            generation.Replay.PublishAdmitted(result);
        }
        finally
        {
            generation.Replay.Exit();
        }
    }

    /// <summary>Closes admission before disconnecting and draining terminal delivery.</summary>
    /// <param name="expected">Originating generation, or null for direct signals.</param>
    /// <param name="error">Terminal error, or null for completion.</param>
    /// <param name="direct">Whether to reject a call after owner disposal.</param>
    internal void Terminate(TransformingSubjectGeneration<TSource, TResult>? expected, Exception? error, bool direct)
    {
        TransformingSubjectGeneration<TSource, TResult> generation;
        lock (_gate)
        {
            if (direct) ObjectDisposedException.ThrowIf(_disposed, this);
            if (_disposed || _terminal || expected?.Closed == true) return;
            _terminal = true;
            generation = _current ??= new(this, _gate);
            generation.Closed = true;
            generation.Replay.Stop(error);
        }
        try { generation.Connection.Dispose(); }
        finally { generation.Replay.Drain(); }
    }

    /// <summary>Retires the last-consumer generation before releasing external resources.</summary>
    /// <param name="generation">Reserved generation.</param>
    internal void Release(TransformingSubjectGeneration<TSource, TResult> generation)
    {
        lock (_gate)
        {
            generation.Consumers--;
            if (generation.Consumers != 0 || generation.Closed || _disposed || _terminal) return;

            // Retire before external disposal: a replacement never shares this connection or replay.
            generation.Closed = true;
            _current = null;
            generation.Replay.Stop(null);
        }
        try { generation.Connection.Dispose(); }
        finally { generation.Replay.Retire(); }
    }

    void Connect(TransformingSubjectGeneration<TSource, TResult> generation)
    {
        ISubject<TSource> source;
        try
        {
            lock (_gate)
            {
                if (generation.Closed || _disposed) return;
            }
            source = sourceFactory() ?? throw new InvalidOperationException("The source factory returned null.");
            lock (_gate)
            {
                if (generation.Closed || _disposed) return;
            }
            generation.Connection.Disposable = source.Subscribe(generation);
        }
        catch (Exception error)
        {
            Terminate(generation, error, false);
        }
    }
}
