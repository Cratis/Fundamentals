// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Cratis.Reflection;
using Cratis.Types;

namespace Cratis.Concepts;

/// <summary>
/// Provides extensions related to working with <see cref="ITypes"/>.
/// </summary>
public static class TypesExtensions
{
    /// <summary>
    /// Register type converters for all <see cref="ConceptAs{T}"/> types.
    /// </summary>
    /// <param name="types"><see cref="ITypes"/> to extend.</param>
    /// <returns><see cref="ITypes"/> for continuation.</returns>
    [RequiresDynamicCode("Uses MakeGenericType to construct type converters for concept types.")]
    public static ITypes RegisterTypeConvertersForConcepts(this ITypes types)
    {
        foreach (var conceptType in types.FindMultiple(typeof(ConceptAs<>)))
        {
            RegisterTypeConverter(conceptType);
        }

        return types;
    }

    /// <summary>
    /// Register type converters for all <see cref="ConceptAs{T}"/> types in the <see cref="Assembly"/>.
    /// </summary>
    /// <param name="assembly"><see cref="Assembly"/> to get the <see cref="ConceptAs{T}"/> types to extend.</param>
    [RequiresUnreferencedCode("Scans all types in the assembly to find concept types.")]
    [RequiresDynamicCode("Uses MakeGenericType to construct type converters for concept types.")]
    public static void RegisterTypeConvertersForConcepts(this Assembly assembly)
    {
        foreach (var conceptType in assembly.GetTypes().Where(t => t.IsConcept()))
        {
            RegisterTypeConverter(conceptType);
        }
    }

    [RequiresDynamicCode("Uses MakeGenericType to construct the type converter for the concept type.")]
    static void RegisterTypeConverter(Type conceptType)
    {
        var typeConverterType = typeof(ConceptAsTypeConverter<,>).MakeGenericType(conceptType, conceptType.GetConceptValueType());
        if (!conceptType.HasAttribute<TypeConverterAttribute>())
        {
            TypeDescriptor.AddAttributes(conceptType, new TypeConverterAttribute(typeConverterType));
        }
    }
}
