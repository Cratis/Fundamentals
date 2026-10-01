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
    static readonly bool _isFlags = typeof(T).IsDefined(typeof(FlagsAttribute), false);
    static readonly int _declaredFlags = _isFlags
        ? Enum.GetValues<T>().Aggregate(0, (mask, value) => mask | Convert.ToInt32(value))
        : 0;

    /// <inheritdoc/>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (!IsAcceptable(intValue))
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

    /// <summary>
    /// Decides whether a numeric value may be converted to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The numeric value read from the document.</param>
    /// <returns>True if the value can be represented by the enum, false otherwise.</returns>
    /// <remarks>
    /// A [Flags] enum is a set, so a combination of declared flags is a legitimate value even though no single
    /// member carries it - Enum.IsDefined answers false for every combination, which made Write and Read
    /// disagree: the combination was written happily and then rejected on the way back in. Any bit outside the
    /// declared flags is still refused, so the check stays a real one.
    /// </remarks>
    static bool IsAcceptable(int value) =>
        _isFlags
            ? (value & ~_declaredFlags) == 0
            : Enum.IsDefined(typeof(T), value);
}
