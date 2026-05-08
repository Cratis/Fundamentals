// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Extension methods for <see cref="ITypeSymbol"/> that classify types for DI analysis.
/// </summary>
internal static class TypeSymbolExtensions
{
    static readonly HashSet<string> _primitiveTypeNames =
    [
        "global::System.Decimal",
        "global::System.Guid",
        "global::System.DateOnly",
        "global::System.TimeOnly",
        "global::System.DateTime",
        "global::System.DateTimeOffset",
        "global::System.TimeSpan"
    ];

    /// <summary>
    /// Returns whether the type is primitive-like and therefore cannot be resolved from a DI container.
    /// Covers all C# built-in value types, strings, nullable wrappers, and common value-type primitives.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is primitive-like; otherwise <see langword="false"/>.</returns>
    public static bool IsPrimitiveLike(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return true;
        }

        if (type.SpecialType is SpecialType.System_Boolean or
            SpecialType.System_Byte or
            SpecialType.System_SByte or
            SpecialType.System_Char or
            SpecialType.System_Double or
            SpecialType.System_Single or
            SpecialType.System_Int16 or
            SpecialType.System_Int32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt16 or
            SpecialType.System_UInt32 or
            SpecialType.System_UInt64 or
            SpecialType.System_String)
        {
            return true;
        }

        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return _primitiveTypeNames.Contains(fullName);
    }
}
