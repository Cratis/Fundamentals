// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cratis.Types;

#nullable disable

namespace Cratis.Concepts;

/// <summary>
/// Factory to create an instance of a <see cref="ConceptAs{T}"/> from the Type and Underlying value.
/// </summary>
public static class ConceptFactory
{
    static readonly ConcurrentDictionary<Type, Creator> _creators = new();

    /// <summary>
    /// Creates an instance of a <see cref="ConceptAs{T}"/> given the type and underlying value.
    /// </summary>
    /// <param name="type">Type of the ConceptAs to create.</param>
    /// <param name="value">Value to give to this instance.</param>
    /// <returns>An instance of a ConceptAs with the specified value.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Concept types always have the necessary constructors preserved by callers.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TypeConversion operates on primitive concept value types that are always preserved.")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "TypeConversion operates on primitive concept value types that are always preserved.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TypeConversion uses MakeGenericMethod on well-known BCL types (JsonValue.GetValue).")]
    public static object CreateConceptInstance(Type type, object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!_creators.TryGetValue(type, out var creator))
        {
            creator = _creators.GetOrAdd(type, static t => CreateCreator(t));
        }

        value = TypeConversion.Convert(creator.ValueType, value);
        var instance = creator.Create(value);

        // The constructor normally already assigned the value. Only fall back to reflection when it did not end up
        // holding exactly the converted value (for instance a derived concept that normalizes it).
        if (instance is not IConceptValueAccessor accessor || !Equals(accessor.ValueAsObject, value))
        {
            ConceptMap.GetValuePropertyInfo(type).SetValue(instance, value, null);
        }

        return instance;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Concept types always have the necessary constructors preserved by callers.")]
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Concept types always have the necessary constructors preserved by callers.")]
    static Creator CreateCreator(Type type)
    {
        var valueType = ConceptMap.GetConceptValueType(type);
        var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, binder: null, [valueType], modifiers: null);
        if (constructor is null)
        {
            // No exact match; leave overload resolution (e.g. widening conversions) to the runtime binder like before.
            return new Creator(valueType, v => Activator.CreateInstance(type, v));
        }

        Func<object, object> create = null;
        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            create = TryCompile(constructor, valueType);
        }

        create ??= v => constructor.Invoke([v]);
        return new Creator(valueType, create);
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Only called when RuntimeFeature.IsDynamicCodeSupported is true; otherwise the reflection path is used.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Expression trees only call a constructor that was already resolved through reflection.")]
    static Func<object, object> TryCompile(ConstructorInfo constructor, Type valueType)
    {
        try
        {
            var parameter = Expression.Parameter(typeof(object), "value");
            var body = Expression.Convert(Expression.New(constructor, Expression.Convert(parameter, valueType)), typeof(object));
            var compiled = Expression.Lambda<Func<object, object>>(body, parameter).Compile();

            // Activator.CreateInstance wraps constructor failures in TargetInvocationException; keep that contract.
            return v =>
            {
                try
                {
                    return compiled(v);
                }
                catch (Exception ex)
                {
                    throw new TargetInvocationException(ex);
                }
            };
        }
        catch
        {
            return null;
        }
    }

    sealed record Creator(Type ValueType, Func<object, object> Create);
}
