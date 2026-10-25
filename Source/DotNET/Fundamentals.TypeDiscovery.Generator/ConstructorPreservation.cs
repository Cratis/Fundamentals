// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Selects bound implementation types whose public constructors can be preserved in generated metadata.
/// </summary>
internal static class ConstructorPreservation
{
    /// <summary>
    /// Gets bound concrete classes and structs with public instance constructors represented in IL.
    /// </summary>
    /// <param name="symbols">The discovered type symbols.</param>
    /// <param name="boundImplementations">The implementation expressions in the generated binding lists.</param>
    /// <returns>The distinct implementation expressions whose constructors should be preserved.</returns>
    public static IEnumerable<string> GetImplementationTypes(
        IEnumerable<INamedTypeSymbol> symbols,
        IEnumerable<string> boundImplementations)
    {
        var boundTypes = new HashSet<string>(boundImplementations, StringComparer.Ordinal);
        return symbols
            .Where(CanPreserveConstructors)
            .Select(static symbol => symbol.GetTypeOfExpression())
            .Where(boundTypes.Contains)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static expression => expression, StringComparer.Ordinal);
    }

    /// <summary>
    /// Checks for preservable public constructors. An implicit struct constructor has no IL member to root;
    /// targeting it raises IL2037.
    /// </summary>
    /// <param name="symbol">The bound implementation type.</param>
    /// <returns>Whether a public instance constructor can be preserved.</returns>
    static bool CanPreserveConstructors(INamedTypeSymbol symbol) =>
        symbol.TypeKind is TypeKind.Class or TypeKind.Struct &&
        !symbol.IsStatic &&
        !symbol.IsAbstract &&
        symbol.InstanceConstructors.Any(constructor =>
            constructor.DeclaredAccessibility == Accessibility.Public &&
            (symbol.TypeKind != TypeKind.Struct || !constructor.IsImplicitlyDeclared));
}
