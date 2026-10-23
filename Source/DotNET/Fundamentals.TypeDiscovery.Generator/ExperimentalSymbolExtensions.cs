// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Extension methods for finding the diagnostic ids of <c language="csharp">[Experimental]</c> markings that apply to a type.
/// </summary>
internal static class ExperimentalSymbolExtensions
{
    const string ExperimentalAttributeName = "System.Diagnostics.CodeAnalysis.ExperimentalAttribute";

    /// <summary>
    /// Gets the distinct diagnostic ids of every <c language="csharp">[Experimental]</c> attribute that makes a
    /// <c language="csharp">typeof</c> of this type (or of a type it derives from or implements) raise a diagnostic:
    /// on the type, any containing type, or its module or assembly.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The distinct, ordinally sorted diagnostic ids.</returns>
    public static ImmutableArray<string> GetExperimentalDiagnosticIds(this INamedTypeSymbol type)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var related in type.GetBaseTypes().Concat(type.AllInterfaces))
        {
            AddIds(related, ids);
        }

        return [.. ids.OrderBy(static id => id, StringComparer.Ordinal)];
    }

    static void AddIds(INamedTypeSymbol type, HashSet<string> ids)
    {
        for (var current = (INamedTypeSymbol?)type; current is not null; current = current.ContainingType)
        {
            AddIds(current.GetAttributes(), ids);
        }

        AddIds(type.ContainingModule.GetAttributes(), ids);
        AddIds(type.ContainingAssembly.GetAttributes(), ids);
    }

    static void AddIds(ImmutableArray<AttributeData> attributes, HashSet<string> ids)
    {
        foreach (var attribute in attributes)
        {
            if (attribute.AttributeClass?.ToDisplayString() == ExperimentalAttributeName &&
                attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is string id &&
                !string.IsNullOrWhiteSpace(id))
            {
                ids.Add(id);
            }
        }
    }
}
