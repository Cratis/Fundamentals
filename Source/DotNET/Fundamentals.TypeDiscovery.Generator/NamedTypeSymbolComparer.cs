// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Equality comparer for <see cref="INamedTypeSymbol"/> using Roslyn symbol equality semantics.
/// </summary>
internal sealed class NamedTypeSymbolComparer : IEqualityComparer<INamedTypeSymbol>
{
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static readonly NamedTypeSymbolComparer Instance = new();

    /// <inheritdoc/>
    public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y) => SymbolEqualityComparer.Default.Equals(x, y);

    /// <inheritdoc/>
    public int GetHashCode(INamedTypeSymbol obj) => SymbolEqualityComparer.Default.GetHashCode(obj);
}
