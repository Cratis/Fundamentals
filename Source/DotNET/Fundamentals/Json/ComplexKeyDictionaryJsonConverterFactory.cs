// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Reflection;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverterFactory"/> for dictionaries with complex key types.
/// </summary>
public class ComplexKeyDictionaryJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Dictionary interfaces are BCL types that are always preserved by the runtime.")]
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsDictionary())
        {
            return false;
        }

        var keyType = GetDictionaryKeyType(typeToConvert);
        return IsComplexType(keyType);
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Dictionary interfaces are BCL types that are always preserved by the runtime.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Uses MakeGenericType on ComplexKeyDictionaryJsonConverter<,,>; types are always preserved.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "ComplexKeyDictionaryJsonConverter<,,> has a public parameterless constructor that is always preserved.")]
    [UnconditionalSuppressMessage("Trimming", "IL2071", Justification = "ComplexKeyDictionaryJsonConverter<,,> has a public parameterless constructor that is always preserved.")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var keyType = GetDictionaryKeyType(typeToConvert);
        var valueType = GetDictionaryValueType(typeToConvert);
        var converterType = typeof(ComplexKeyDictionaryJsonConverter<,,>).MakeGenericType(typeToConvert, keyType, valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Dictionary interfaces are BCL types that are always preserved by the runtime.")]
    static Type GetDictionaryKeyType(Type typeToConvert)
    {
        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IDictionary<,>))
        {
            return typeToConvert.GetGenericArguments()[0];
        }

        return typeToConvert.GetKeyType();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Dictionary interfaces are BCL types that are always preserved by the runtime.")]
    static Type GetDictionaryValueType(Type typeToConvert)
    {
        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IDictionary<,>))
        {
            return typeToConvert.GetGenericArguments()[1];
        }

        return typeToConvert.GetValueType();
    }

    static bool IsComplexType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return !type.IsPrimitive &&
               !type.IsEnum &&
               type != typeof(string) &&
               type != typeof(Guid) &&
               type != typeof(DateTime) &&
               type != typeof(DateTimeOffset) &&
               type != typeof(DateOnly) &&
               type != typeof(TimeOnly) &&
               type != typeof(TimeSpan) &&
               type != typeof(decimal) &&
               type != typeof(Uri);
    }

    sealed class ComplexKeyDictionaryJsonConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
        where TKey : notnull
    {
        /// <inheritdoc/>
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Dictionary JSON deserialization uses generic type parameters that are preserved.")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Dictionary JSON deserialization uses generic type parameters that are safe for AOT.")]
        public override TDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var document = JsonDocument.ParseValue(ref reader);
            var dictionary = CreateDictionary(typeToConvert);
            var keySerializationOptions = GetKeySerializationOptions(options);

            foreach (var property in document.RootElement.EnumerateObject())
            {
                var key = JsonSerializer.Deserialize<TKey>(property.Name, keySerializationOptions)!;
                dictionary[key] = property.Value.Deserialize<TValue>(options)!;
            }

            return (TDictionary)(object)dictionary;
        }

        /// <inheritdoc/>
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Dictionary JSON serialization uses generic type parameters that are preserved.")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Dictionary JSON serialization uses generic type parameters that are safe for AOT.")]
        public override void Write(Utf8JsonWriter writer, TDictionary value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            var keySerializationOptions = GetKeySerializationOptions(options);

            foreach (var keyValue in (IDictionary<TKey, TValue>)(object)value)
            {
                var keyAsJson = JsonSerializer.Serialize(keyValue.Key, keySerializationOptions);
                writer.WritePropertyName(keyAsJson);
                JsonSerializer.Serialize(writer, keyValue.Value, options);
            }

            writer.WriteEndObject();
        }

        [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "The dictionary type to create is preserved by the converter factory.")]
        static IDictionary<TKey, TValue> CreateDictionary(Type typeToConvert)
        {
            if (typeToConvert.IsInterface || typeToConvert.IsAbstract)
            {
                return new Dictionary<TKey, TValue>();
            }

            if (Activator.CreateInstance(typeToConvert) is IDictionary<TKey, TValue> dictionary)
            {
                return dictionary;
            }

            return new Dictionary<TKey, TValue>();
        }

        static JsonSerializerOptions GetKeySerializationOptions(JsonSerializerOptions options) => new(options)
        {
            WriteIndented = false
        };
    }
}