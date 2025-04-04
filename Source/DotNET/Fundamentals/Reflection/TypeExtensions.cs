// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Dynamic;
using System.Reflection;

namespace Cratis.Reflection;

/// <summary>
/// Provides a set of methods for working with <see cref="Type">types</see>.
/// </summary>
public static class TypeExtensions
{
    static readonly HashSet<Type> _additionalPrimitiveTypes =
    [
        typeof(decimal),
        typeof(string),
        typeof(Guid),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan)
    ];

    static readonly HashSet<Type> _numericTypes =
    [
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(double),
        typeof(decimal),
        typeof(float)
    ];

    /// <summary>
    /// Get the type string for a <see cref="Type"/>.
    /// </summary>
    /// <param name="type">Type to get from.</param>
    /// <returns>A type string.</returns>
    public static string GetTypeString(this Type type) =>
        (type.Namespace?.StartsWith("System") ?? false) ?
            (type.FullName ?? type.Name) :
            $"{type.FullName ?? type.Name}, {type.Assembly.GetName().Name}";

    /// <summary>
    /// Check if a type has an attribute associated with it.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <typeparam name="T">Attribute type to check for.</typeparam>
    /// <returns>True if there is an attribute, false if not.</returns>
    public static bool HasAttribute<T>(this Type type)
        where T : Attribute
    {
        var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(T), false).ToArray();
        return attributes.Length == 1;
    }

    /// <summary>
    /// Check if a type is nullable or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is nullable, false if not.</returns>
    public static bool IsNullable(this Type type)
    {
        while (!type.Equals(typeof(object)))
        {
            if (type.GetTypeInfo().IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return true;
            }

            if (type.BaseType is null) break;

            type = type.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Check whether or not a type is in a nullable context.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>True if it is in a nullable context, false if not.</returns>
    public static bool IsNullableContext(this Type type) =>
        type.GetCustomAttributes().Any(_ => _.GetType().FullName == "System.Runtime.CompilerServices.NullableContextAttribute");

    /// <summary>
    /// Check whether or not a member that implements <see cref="ICustomAttributeProvider"/> is a nullable reference type.
    /// </summary>
    /// <param name="member">Member that implements <see cref="ICustomAttributeProvider"/> to check.</param>
    /// <returns>True if it nullable, false if not.</returns>
    public static bool IsNullableReferenceType(this ICustomAttributeProvider member) =>
        member.GetCustomAttributes(false).Any(_ => _.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");

    /// <summary>
    /// Gets the element type of an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <param name="asyncEnumerableType">The <see cref="Type"/> to get from.</param>
    /// <returns>The element type.</returns>
    public static Type GetAsyncEnumerableElementType(this Type asyncEnumerableType)
    {
        if (asyncEnumerableType.IsGenericType && asyncEnumerableType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
        {
            return asyncEnumerableType.GetGenericArguments()[0];
        }

        return asyncEnumerableType.GetInterfaces()
            .Where(t => t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            .Select(t => t.GenericTypeArguments[0]).FirstOrDefault()!;
    }

    /// <summary>
    /// Get the underlying nullable type.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to get from.</param>
    /// <returns>Underlying nullable type.</returns>
    public static Type GetNullableType(this Type type) => type.GetGenericArguments()[0];

    /// <summary>
    /// Check if a type is a number or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is numeric, false if not.</returns>
    public static bool IsNumericType(this Type type)
    {
        return _numericTypes.Contains(type) ||
               _numericTypes.Contains(Nullable.GetUnderlyingType(type)!);
    }

    /// <summary>
    /// Check if a type is enumerable. Note that string is an IEnumerable, but in this case the string is excluded, as well as ExpandoObject.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is enumerable, false if not an enumerable.</returns>
    public static bool IsEnumerable(this Type type)
    {
        return !type.IsAPrimitiveType() && type != typeof(ExpandoObject) && !type.IsString() && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    /// <summary>
    /// Check if a type is comparable, either implementing <see cref="IComparable"/> or <see cref="IComparable{T}"/>.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is comparable, false if not an comparable.</returns>
    public static bool IsComparable(this Type type)
    {
        return type.Implements(typeof(IComparable)) || type.ImplementsOpenGeneric(typeof(IComparable<>));
    }

    /// <summary>
    /// Check if the type is of the type specified in the generic param.
    /// </summary>
    /// <typeparam name="T">Type of the instance.</typeparam>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a date, false if not.</returns>
    public static bool Is<T>(this Type type)
    {
        return type == typeof(T) || Nullable.GetUnderlyingType(type) == typeof(T);
    }

    /// <summary>
    /// Check if a type is a Date or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a date, false if not.</returns>
    public static bool IsDate(this Type type)
    {
        return Is<DateTime>(type);
    }

    /// <summary>
    /// Check if a type is a DateOnly or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a date only, false if not.</returns>
    public static bool IsDateOnly(this Type type)
    {
        return Is<DateOnly>(type);
    }

    /// <summary>
    /// Check if a type is a TimeOnly or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a time only, false if not.</returns>
    public static bool IsTimeOnly(this Type type)
    {
        return Is<TimeOnly>(type);
    }

    /// <summary>
    /// Check if a type is a DateTimeOffset or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a date, false if not.</returns>
    public static bool IsDateTimeOffset(this Type type)
    {
        return Is<DateTimeOffset>(type);
    }

    /// <summary>
    /// Check if a type is a Boolean or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a boolean, false if not.</returns>
    public static bool IsBoolean(this Type type)
    {
        return Is<bool>(type);
    }

    /// <summary>
    /// Check if a type is a String or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a string, false otherwise.</returns>
    public static bool IsString(this Type type)
    {
        return Is<string>(type);
    }

    /// <summary>
    /// Check if a type is a Guid or not.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if type is a Guid, false otherwise.</returns>
    public static bool IsGuid(this Type type)
    {
        return Is<Guid>(type);
    }

    /// <summary>
    /// Gets all the public properties with setters.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to get settable properties for.</param>
    /// <returns>Settable <see cref="PropertyInfo">properties</see>.</returns>
    public static PropertyInfo[] GetSettableProperties(this Type type)
    {
        return [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite)];
    }

    /// <summary>
    /// Gets the element type of an enumerable.
    /// </summary>
    /// <param name="enumerableType">The <see cref="Type">type of the enumerable</see>.</param>
    /// <returns>Enumerable element <see cref="Type"/>.</returns>
    /// <remarks>
    /// https://stackoverflow.com/questions/906499/getting-type-t-from-ienumerablet.
    /// </remarks>
    public static Type GetEnumerableElementType(this Type enumerableType)
    {
        if (enumerableType.IsArray)
        {
            return enumerableType.GetElementType()!;
        }

        if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return enumerableType.GetGenericArguments()[0];
        }

        return enumerableType.GetInterfaces()
            .Where(t => t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(t => t.GenericTypeArguments[0]).FirstOrDefault()!;
    }

    /// <summary>
    /// Check if a type implements a specific interface.
    /// </summary>
    /// <typeparam name="T">Interface to check for.</typeparam>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if the type implements the interface, false if not.</returns>
    public static bool HasInterface<T>(this Type type) => type.HasInterface(typeof(T));

    /// <summary>
    /// Check if a type implements a specific interface.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <param name="interfaceType">Interface to check for.</param>
    /// <returns>True if the type implements the interface, false if not.</returns>
    public static bool HasInterface(this Type type, Type interfaceType)
    {
        bool DoesGenericParametersMatch(Type typeToCheck)
        {
            return typeToCheck.IsGenericType &&
                interfaceType.GetTypeInfo().GetGenericTypeDefinition().Equals(typeToCheck.GetGenericTypeDefinition());
        }

        if (interfaceType.IsGenericTypeDefinition)
        {
            return type.GetTypeInfo()
                        .ImplementedInterfaces
                        .Count(DoesGenericParametersMatch) == 1;
        }

        bool MatchesInterface(Type typeToCheck) => typeToCheck.Equals(interfaceType);

        return type.GetTypeInfo()
                    .ImplementedInterfaces
                    .Count(MatchesInterface) == 1;
    }

    /// <summary>
    /// Check if a type derives from an open generic type.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <param name="openGenericType">Open generic <see cref="Type"/> to check for.</param>
    /// <returns>True if type matches the open generic <see cref="Type"/>.</returns>
    public static bool IsDerivedFromOpenGeneric(this Type type, Type openGenericType)
    {
        var typeToCheck = type;
        while (typeToCheck != null && typeToCheck != typeof(object))
        {
            var currentType = typeToCheck.GetTypeInfo().IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck;
            if (openGenericType == currentType)
                return true;

            typeToCheck = typeToCheck.GetTypeInfo().BaseType;
        }

        return false;
    }

    /// <summary>
    /// Check if a type implements an open generic type.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <param name="openGenericType">Open generic <see cref="Type"/> to check for.</param>
    /// <returns>True if type implements the open generic <see cref="Type"/>.</returns>
    public static bool ImplementsOpenGeneric(this Type type, Type openGenericType)
    {
        var openGenericTypeInfo = openGenericType.GetTypeInfo();
        var typeInfo = type.GetTypeInfo();

        return typeInfo.GetInterfaces()
            .Any(i => i.GetTypeInfo().IsGenericType && i.GetTypeInfo().GetGenericTypeDefinition().GetTypeInfo() == openGenericTypeInfo);
    }

    /// <summary>
    /// Check if a type is a "primitive" type.  This is not just dot net primitives but basic types like string, decimal, datetime,
    /// that are not classified as primitive types.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if <see cref="Type"/> is a primitive type.</returns>
    public static bool IsAPrimitiveType(this Type type)
    {
        return type.GetTypeInfo().IsPrimitive
                || type.IsNullable() || _additionalPrimitiveTypes.Contains(type);
    }

    /// <summary>
    /// Check if a type implements another type - supporting interfaces, abstract types, with or without generics.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <param name="super">Super / parent type to check against.</param>
    /// <returns>True if derived, false if not.</returns>
    public static bool Implements(this Type type, Type super)
    {
        return type.AllBaseAndImplementingTypes().Contains(super);
    }

    /// <summary>
    /// Returns all base types of a given type, both open and closed generic (if any), including itself.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to get for.</param>
    /// <returns>All base and implementing <see cref="Type">types</see>.</returns>
    public static IEnumerable<Type> AllBaseAndImplementingTypes(this Type type)
    {
        return type.BaseTypes()
            .Concat(type.GetTypeInfo().GetInterfaces())
            .SelectMany(ThisAndMaybeOpenType)
            .Where(t => t != type && t != typeof(object));
    }

    /// <summary>
    /// Indicates whether the Type has any public properties to get or set state.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to check.</param>
    /// <returns>True if there are public properties (get or set), false otherwise.</returns>
    public static bool HasVisibleProperties(this Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Length > 0;
    }

    /// <summary>
    /// Get <see cref="ITypeInfo"/> from <see cref="Type"/>.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to get from.</param>
    /// <returns>The <see cref="ITypeInfo"/>.</returns>
    public static ITypeInfo GetTypeInfoDetails(this Type type)
    {
        var typeInfoType = typeof(TypeInfo<>).MakeGenericType(type);
        var instanceField = typeInfoType.GetTypeInfo().GetField("Instance", BindingFlags.Public | BindingFlags.Static);
        return (ITypeInfo)instanceField!.GetValue(null)!;
    }

    /// <summary>
    /// Check whether or not a type is a record type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>True if record, false if not.</returns>
    public static bool IsRecord(this Type type) => type.GetTypeInfo().DeclaredProperties.Any(x => x.Name == "EqualityContract");

    static IEnumerable<Type> BaseTypes(this Type type)
    {
        var currentType = type;
        while (currentType != null)
        {
            yield return currentType;
            currentType = currentType.GetTypeInfo().BaseType;
        }
    }

    static IEnumerable<Type> ThisAndMaybeOpenType(Type type)
    {
        yield return type;
        if (type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().ContainsGenericParameters)
        {
            yield return type.GetGenericTypeDefinition();
        }
    }
}
