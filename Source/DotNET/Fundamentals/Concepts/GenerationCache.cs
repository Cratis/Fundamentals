// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Cratis.Concepts;

/// <summary>
/// Retains a bounded set of shared calculations without locking cache hits.
/// </summary>
/// <typeparam name="TKey">The complete input identity of a calculation.</typeparam>
/// <typeparam name="TValue">The calculated value, with the constructor metadata required by Lazy.</typeparam>
internal sealed class GenerationCache<TKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TValue>
    where TKey : notnull
{
    const int Capacity = 128;
    readonly ConcurrentDictionary<TKey, Lazy<TValue>> _entries = new();
#if NET9_0_OR_GREATER
    readonly Lock _missLock = new();
#else
    readonly object _missLock = new();
#endif

    /// <summary>
    /// Gets a retained value or shares its construction with concurrent callers.
    /// </summary>
    /// <param name="key">The input identity.</param>
    /// <param name="create">The calculation used for a missing key.</param>
    /// <returns>The completed value; failed calculations are not retained.</returns>
    internal TValue Get(TKey key, Func<TKey, TValue> create)
    {
        if (!_entries.TryGetValue(key, out var entry))
        {
            lock (_missLock)
            {
                if (!_entries.TryGetValue(key, out entry))
                {
                    if (_entries.Count == Capacity)
                    {
                        _entries.Clear();
                    }

                    entry = new(() => create(key), LazyThreadSafetyMode.ExecutionAndPublication);
                    _entries[key] = entry;
                }
            }
        }

        try
        {
            return entry.Value;
        }
        catch
        {
            // Do not retain failed constructions. Remove only this particular lazy,
            // not a replacement published after a concurrent cache clear.
            ((ICollection<KeyValuePair<TKey, Lazy<TValue>>>)_entries).Remove(new(key, entry));
            throw;
        }
    }
}
