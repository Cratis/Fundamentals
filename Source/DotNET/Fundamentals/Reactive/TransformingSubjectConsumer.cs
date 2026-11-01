// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive;

/// <summary>Releases a consumer reservation and its assignment-safe replay attachment exactly once.</summary>
/// <typeparam name="TSource">Source value type.</typeparam>
/// <typeparam name="TResult">Result value type.</typeparam>
/// <param name="owner">Generation owner.</param>
/// <param name="generation">Reserved generation.</param>
/// <param name="attachment">Owned replay attachment.</param>
/// <param name="counted">Whether the reservation belongs to an open generation.</param>
internal sealed class TransformingSubjectConsumer<TSource, TResult>(SubscriberScopedTransformingSubject<TSource, TResult> owner, TransformingSubjectGeneration<TSource, TResult> generation, TransformingSubjectSubscription<TResult> attachment, bool counted) : IDisposable
{
    int _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        try { attachment.Dispose(); }
        finally
        {
            if (counted) owner.Release(generation);
        }
    }

    /// <summary>Attaches the reservation outside the owner's gate.</summary>
    internal void Attach() => attachment.Attach();
}
