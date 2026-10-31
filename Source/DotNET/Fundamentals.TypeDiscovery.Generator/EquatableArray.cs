// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Immutable;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// An immutable sequence with structural equality for incremental generator values.
/// </summary>
/// <param name="values">The immutable sequence.</param>
/// <typeparam name="T">The element type.</typeparam>
internal readonly struct EquatableArray<T>(ImmutableArray<T> values) : IEquatable<EquatableArray<T>>, IEnumerable<T>
{
    /// <inheritdoc/>
    public bool Equals(EquatableArray<T> other) => this.SequenceEqual(other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var value in this)
        {
            hash = unchecked((hash * 31) + (value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(value)));
        }

        return hash;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(values.IsDefault ? [] : values)).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
