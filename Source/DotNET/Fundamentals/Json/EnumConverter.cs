// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Json;

/// <summary>
/// Represents an <see cref="JsonConverter{T}"/> for converting enums.
/// </summary>
/// <typeparam name="T">Type of enum.</typeparam>
public class EnumConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    /// <inheritdoc/>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (!Enum.IsDefined(typeof(T), intValue))
            {
                throw new JsonException($"Unable to convert \"{intValue}\" to Enum \"{typeof(T).FullName}\". Value is not defined.");
            }

            return (T)Enum.ToObject(typeof(T), intValue);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (Enum.TryParse(stringValue, true, out T result))
            {
                return result;
            }

            throw new JsonException($"Unable to convert \"{stringValue}\" to Enum \"{typeof(T).FullName}\".");
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing an Enum.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}
