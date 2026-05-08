// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Extension methods for <see cref="INamespaceSymbol"/> and <see cref="INamedTypeSymbol"/>
/// that enumerate types within a namespace or nested type hierarchy.
/// </summary>
internal static class NamespaceSymbolExtensions
{
    /// <summary>
    /// Recursively enumerates all named types within a namespace, including types in child namespaces.
    /// </summary>
    /// <param name="namespace">The namespace to walk.</param>
    /// <returns>All <see cref="INamedTypeSymbol"/> instances found.</returns>
    public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamespaceSymbol @namespace)
    {
        foreach (var member in @namespace.GetMembers())
        {
            if (member is INamespaceSymbol childNamespace)
            {
                foreach (var type in childNamespace.GetAllNamedTypes())
                {
                    yield return type;
                }
            }

            if (member is INamedTypeSymbol namedType)
            {
                foreach (var type in namedType.GetAllNamedTypes())
                {
                    yield return type;
                }
            }
        }
    }

    /// <summary>
    /// Enumerates a named type and all of its nested types recursively.
    /// </summary>
    /// <param name="type">The type to walk.</param>
    /// <returns>The type itself followed by all nested types.</returns>
    public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamedTypeSymbol type)
    {
        yield return type;

        foreach (var childType in type.GetTypeMembers())
        {
            foreach (var nestedType in childType.GetAllNamedTypes())
            {
                yield return nestedType;
            }
        }
    }
}
