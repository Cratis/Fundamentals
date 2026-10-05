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
    static readonly ulong _declaredFlags = _isFlags
        ? Enum.GetValues<T>().Aggregate(0UL, (mask, value) => mask | EnumJson.ToBits(value, typeof(T)))
        : 0UL;

    /// <inheritdoc/>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var enumValue = (T)EnumJson.ReadEnum(ref reader, typeof(T));
            if (!IsAcceptable(enumValue))
            {
                throw new JsonException($"Unable to convert \"{enumValue.ToString("D")}\" to Enum \"{typeof(T).FullName}\". Value is not defined.");
            }

            return enumValue;
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
        EnumJson.WriteEnum(writer, value, typeof(T));
    }

    /// <summary>
    /// Decides whether a numeric value may be converted to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The enum value read from the document.</param>
    /// <returns>True if the value can be represented by the enum, false otherwise.</returns>
    /// <remarks>
    /// A [Flags] enum is a set, so a combination of declared flags is a legitimate value even though no single
    /// member carries it - Enum.IsDefined answers false for every combination, which made Write and Read
    /// disagree: the combination was written happily and then rejected on the way back in. Any bit outside the
    /// declared flags is still refused, so the check stays a real one.
    /// </remarks>
    static bool IsAcceptable(T value) =>
        _isFlags
            ? (EnumJson.ToBits(value, typeof(T)) & ~_declaredFlags) == 0
            : Enum.IsDefined(value);
}
