// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
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
    public static void RegisterTypeConvertersForConcepts(this Assembly assembly)
    {
        foreach (var conceptType in assembly.GetTypes().Where(t => t.IsConcept()))
        {
            RegisterTypeConverter(conceptType);
        }
    }

    static void RegisterTypeConverter(Type conceptType)
    {
        var typeConverterType = typeof(ConceptAsTypeConverter<,>).MakeGenericType(conceptType, conceptType.GetConceptValueType());
        if (!conceptType.HasAttribute<TypeConverterAttribute>())
        {
            TypeDescriptor.AddAttributes(conceptType, new TypeConverterAttribute(typeConverterType));
        }
    }
}
