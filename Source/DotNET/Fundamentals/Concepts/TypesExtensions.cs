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
    static readonly HashSet<Type> _registered = [];

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
        // TypeDescriptor is process-global, so one registration per concept type is the whole job. Every
        // repeated AddAttributes wrapped another provider around the type and took TypeDescriptor's global
        // lock to do it, which a host configuring many containers - a spec suite - paid per container.
        lock (_registered)
        {
            if (!_registered.Add(conceptType) || conceptType.HasAttribute<TypeConverterAttribute>())
            {
                return;
            }

            var typeConverterType = typeof(ConceptAsTypeConverter<,>).MakeGenericType(conceptType, conceptType.GetConceptValueType());
            TypeDescriptor.AddAttributes(conceptType, new TypeConverterAttribute(typeConverterType));
        }
    }
}
