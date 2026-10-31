// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Creates symbol-free identities without merging type arguments from different owners or assemblies.
/// </summary>
internal static class TypeIdentity
{
    /// <summary>Identifies a type, including constructed containing types and type-parameter owners.</summary>
    /// <param name="type">The type to identify.</param>
    /// <returns>A stable structural identity that retains no symbols.</returns>
    public static string Create(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol parameter => $"{parameter.ContainingAssembly.Identity}:{parameter.ContainingSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}!{parameter.TypeParameterKind}:{parameter.Ordinal}",
        INamedTypeSymbol named => NamedIdentity(named.TupleUnderlyingType ?? named),
        IArrayTypeSymbol array => $"{Create(array.ElementType)}[{array.Rank}:{array.IsSZArray}]",
        IPointerTypeSymbol pointer => $"{Create(pointer.PointedAtType)}*",
        _ => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
    };

    static string NamedIdentity(INamedTypeSymbol type)
    {
        var definition = type.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var containing = type.ContainingType is { } owner ? Create(owner) : string.Empty;
        var arguments = string.Join(",", type.TypeArguments.Select(Create));
        return $"{type.ContainingAssembly.Identity}:{definition}[{containing}]<{arguments}>";
    }
}
