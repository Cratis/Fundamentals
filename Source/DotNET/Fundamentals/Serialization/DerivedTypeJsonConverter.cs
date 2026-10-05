// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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

                foreach (var property in type.GetProperties())
                {
                    var propertyValue = property.GetValue(value);
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
                    writer.WritePropertyName(property.Key);
                    JsonSerializer.Serialize(writer, property.Value.Value, property.Value.SerializationType, options);
                }
                writer.WriteEndObject();
                break;
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Declared property collection interfaces are preserved with the registered derived types.")]
    bool HasDeclaredFamily(Type type) =>
        _derivedTypes.HasDerivatives(type) ||
        (type.IsConstructedGenericType && type.GetInterfaces().Prepend(type).Any(candidate =>
            candidate.IsGenericType &&
            ((candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>) && _derivedTypes.HasDerivatives(candidate.GetGenericArguments()[0])) ||
             ((candidate.GetGenericTypeDefinition() == typeof(IDictionary<,>) || candidate.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)) && _derivedTypes.HasDerivatives(candidate.GetGenericArguments()[1])))));
}
