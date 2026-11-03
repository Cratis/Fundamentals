// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Cratis.Strings;

namespace Cratis.Serialization;

/// <summary>
/// Represents a <see cref="JsonConverter{T}"/> for converting types that are adorned with the <see cref="DerivedTypeAttribute"/>.
/// </summary>
/// <typeparam name="T">The interface the derived type implements to convert.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DerivedTypeJsonConverter{T}"/> class.
/// </remarks>
/// <param name="derivedTypes"><see cref="IDerivedTypes"/> to use for discovering correct type.</param>
public class DerivedTypeJsonConverter<T>(IDerivedTypes derivedTypes) : JsonConverter<T>
{
    /// <summary>
    /// The property used in JSON to identify the derived type id.
    /// </summary>
    public const string DerivedTypeIdProperty = "_derivedTypeId";

    readonly IDerivedTypes _derivedTypes = derivedTypes;
    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "ConditionalWeakTable has no collection expression support.")]
    readonly ConditionalWeakTable<JsonSerializerOptions, ConcurrentDictionary<Type, IncludedProperty[]>> _includedProperties = new();

    /// <inheritdoc/>
    /// <remarks>
    /// A missing derived type identifier returns the default value so payloads written without nested
    /// identifiers by earlier versions remain readable when replaying persisted events.
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Derived type JSON deserialization uses types registered at startup that are preserved.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Derived type JSON deserialization uses types registered at startup that are safe for AOT.")]
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader);

        if (document.RootElement.TryGetProperty(DerivedTypeIdProperty, out var value))
        {
            var derivedTypeId = (DerivedTypeId)value.GetString()!;
            var derivedType = _derivedTypes.GetDerivedTypeFor(typeToConvert, derivedTypeId);
            var instance = document.Deserialize(derivedType, options);
            return (T)instance!;
        }
        return default!;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Nested children and collections declared with a derived type family use their declared types
    /// to preserve type identifiers and the converter's naming rules. Other properties use runtime types.
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Derived type JSON serialization uses types registered at startup that are preserved.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Derived type JSON serialization accesses well-known type properties that are preserved.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Derived type JSON serialization uses types registered at startup that are safe for AOT.")]
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                JsonSerializer.Serialize(writer, null!, options);
                break;

            default:
                var type = value.GetType();
                var properties = new Dictionary<string, (object? Value, Type SerializationType)>();

                foreach (var included in GetIncludedProperties(type, options))
                {
                    var property = included.Property;
                    var propertyValue = property.GetValue(value);
                    if (included.ShouldIgnore(value, propertyValue))
                    {
                        continue;
                    }

                    var serializationType = HasDeclaredFamily(property.PropertyType)
                        ? property.PropertyType
                        : propertyValue?.GetType() ?? property.PropertyType;
                    properties[property.Name.ToCamelCase()] = (propertyValue, serializationType);
                }

                var derivedTypeAttribute = type.GetCustomAttribute<DerivedTypeAttribute>();
                if (derivedTypeAttribute is not null)
                {
                    properties[DerivedTypeIdProperty] = (derivedTypeAttribute.Identifier.ToString(), typeof(string));
                }

                writer.WriteStartObject();
                foreach (var property in properties)
                {
                    writer.WritePropertyName(options?.DictionaryKeyPolicy?.ConvertName(property.Key) ?? property.Key);
                    JsonSerializer.Serialize(writer, property.Value.Value, property.Value.SerializationType, options);
                }
                writer.WriteEndObject();
                break;
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Derived type JSON serialization accesses well-known type properties that are preserved.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Derived type JSON serialization uses types registered at startup that are preserved.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Derived type JSON serialization uses types registered at startup that are safe for AOT.")]
    static IncludedProperty[] ResolveIncludedProperties(Type type, JsonSerializerOptions? options)
    {
        options ??= JsonSerializerOptions.Default;
        var clrProperties = type.GetProperties();
        var typeInfo = TryGetTypeInfo(type, options);

        // A type with a custom converter has no property metadata; keep all public properties as before.
        if (typeInfo is not null && typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return [.. clrProperties.Select(_ => CreateIncludedProperty(_, null, options)).OfType<IncludedProperty>()];
        }

        return [.. clrProperties.Select(_ => CreateIncludedProperty(_, typeInfo, options)).OfType<IncludedProperty>()];
    }

    /// <summary>
    /// Without metadata (e.g. property names that collide under the options naming policy) falls back to the attributes.
    /// </summary>
    /// <param name="property">The CLR property.</param>
    /// <param name="typeInfo">The JSON metadata, if available.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The included property, or null when it is ignored.</returns>
    static IncludedProperty? CreateIncludedProperty(PropertyInfo property, JsonTypeInfo? typeInfo, JsonSerializerOptions options)
    {
        var ignoreAttribute = property.GetCustomAttribute<JsonIgnoreAttribute>();
        var jsonProperty = typeInfo?.Properties.FirstOrDefault(_ => _.AttributeProvider is PropertyInfo info && info.Name == property.Name && info.DeclaringType == property.DeclaringType);
        var isIncluded = typeInfo is null ? ignoreAttribute?.Condition != JsonIgnoreCondition.Always : jsonProperty is not null;
        if (!isIncluded)
        {
            return null;
        }

        var isWritable = jsonProperty is null ? property.SetMethod?.IsPublic == true : jsonProperty.Set is not null;
        var ignoredAsReadOnly = !isWritable && IgnoredAsReadOnly(property, ignoreAttribute, options);
        return new IncludedProperty(
            property,
            jsonProperty?.ShouldSerialize,
            ignoreAttribute?.Condition ?? options.DefaultIgnoreCondition,
            ignoredAsReadOnly);
    }

    static bool IgnoredAsReadOnly(PropertyInfo property, JsonIgnoreAttribute? ignoreAttribute, JsonSerializerOptions options) =>
        options.IgnoreReadOnlyProperties &&
        ignoreAttribute?.Condition != JsonIgnoreCondition.Never &&
        !IsCollection(property.PropertyType, options);

    /// <summary>
    /// System.Text.Json keeps read-only collection properties even when IgnoreReadOnlyProperties is set.
    /// </summary>
    /// <param name="type">The property type.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>True if the type is serialized as a collection.</returns>
    static bool IsCollection(Type type, JsonSerializerOptions options) =>
        TryGetTypeInfo(type, options)?.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Derived type JSON serialization uses types registered at startup that are preserved.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Derived type JSON serialization uses types registered at startup that are safe for AOT.")]
    static JsonTypeInfo? TryGetTypeInfo(Type type, JsonSerializerOptions options)
    {
        try
        {
            return options.GetTypeInfo(type);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Derived type JSON serialization uses types registered at startup that are preserved.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Derived type JSON serialization uses types registered at startup that are safe for AOT.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Derived type JSON serialization accesses well-known type properties that are preserved.")]
    IncludedProperty[] GetIncludedProperties(Type type, JsonSerializerOptions options) =>
        _includedProperties
            .GetValue(options ?? JsonSerializerOptions.Default, _ => new ConcurrentDictionary<Type, IncludedProperty[]>())
            .GetOrAdd(type, ResolveIncludedProperties, options);

    bool HasDeclaredFamily(Type type) => HasDeclaredFamily(type, [], 0);

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Declared property collection interfaces are preserved with the registered derived types.")]
    bool HasDeclaredFamily(Type type, HashSet<Type> visitedTypes, int depth)
    {
        if (type == typeof(string) || depth >= 32 || !visitedTypes.Add(type))
        {
            return false;
        }

        if (_derivedTypes.HasDerivatives(type))
        {
            return true;
        }

        if (type.IsArray)
        {
            return HasDeclaredFamily(type.GetElementType()!, visitedTypes, depth + 1);
        }

        return type.GetInterfaces().Prepend(type).Any(candidate =>
            candidate.IsGenericType &&
            ((candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>) && HasDeclaredFamily(candidate.GetGenericArguments()[0], visitedTypes, depth + 1)) ||
             ((candidate.GetGenericTypeDefinition() == typeof(IDictionary<,>) || candidate.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)) && HasDeclaredFamily(candidate.GetGenericArguments()[1], visitedTypes, depth + 1))));
    }

    sealed record IncludedProperty(PropertyInfo Property, Func<object, object?, bool>? ShouldSerialize, JsonIgnoreCondition Condition, bool IsReadOnly)
    {
        readonly object? _defaultValue = CreateDefault(Property.PropertyType);

        public bool ShouldIgnore(object instance, object? propertyValue) =>
            IsReadOnly ||
            (ShouldSerialize is not null && !ShouldSerialize(instance, propertyValue)) ||
            IgnoredByCondition(propertyValue);

        bool IgnoredByCondition(object? propertyValue) => Condition switch
        {
            JsonIgnoreCondition.WhenWritingNull => propertyValue is null,
            JsonIgnoreCondition.WhenWritingDefault => propertyValue?.Equals(_defaultValue) ?? true,
            _ => false
        };

        [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Only value types are instantiated, which never need constructors.")]
        [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Only value types are instantiated, which never need constructors.")]
        static object? CreateDefault(Type type) => type.IsValueType && Nullable.GetUnderlyingType(type) is null ? RuntimeHelpers.GetUninitializedObject(type) : null;
    }
}
