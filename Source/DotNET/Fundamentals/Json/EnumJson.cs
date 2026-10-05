// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Json;

/// <summary>
/// Reads and writes enum values as JSON numbers through the enum's underlying type, so every backing type round-trips.
/// </summary>
static class EnumJson
{
    /// <summary>
    /// Reads the current JSON number as a value of the given enum type.
    /// </summary>
    /// <param name="reader">The reader positioned on a number token.</param>
    /// <param name="enumType">The enum type.</param>
    /// <returns>The enum value.</returns>
    /// <exception cref="JsonException">The number is not an integer or does not fit the enum's underlying type.</exception>
    internal static object ReadEnum(ref Utf8JsonReader reader, Type enumType)
    {
        var underlyingType = Enum.GetUnderlyingType(enumType);
        var unsigned = IsUnsigned(underlyingType);
        object number;
        if (unsigned && reader.TryGetUInt64(out var unsignedNumber))
        {
            number = unsignedNumber;
        }
        else if (!unsigned && reader.TryGetInt64(out var signedNumber))
        {
            number = signedNumber;
        }
        else
        {
            var rawNumber = reader.HasValueSequence ? Encoding.UTF8.GetString(reader.ValueSequence) : Encoding.UTF8.GetString(reader.ValueSpan);
            throw new JsonException($"The JSON number '{rawNumber}' is not a valid value for enum '{enumType}' backed by '{underlyingType}'.");
        }

        try
        {
            return Enum.ToObject(enumType, Convert.ChangeType(number, underlyingType));
        }
        catch (OverflowException ex)
        {
            // The number does not fit the enum's underlying type; surface it as a JSON error naming the enum and value.
            throw new JsonException($"The JSON number '{number}' is out of range for enum '{enumType}' backed by '{underlyingType}'.", ex);
        }
    }

    /// <summary>
    /// Writes an enum value as a JSON number.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The enum value.</param>
    /// <param name="enumType">The enum type.</param>
    internal static void WriteEnum(Utf8JsonWriter writer, object value, Type enumType)
    {
        if (IsUnsigned(Enum.GetUnderlyingType(enumType)))
        {
            writer.WriteNumberValue(Convert.ToUInt64(value));
        }
        else
        {
            writer.WriteNumberValue(Convert.ToInt64(value));
        }
    }

    /// <summary>
    /// Gets the value's bit pattern, sign-extended for signed backing types.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <param name="enumType">The enum type.</param>
    /// <returns>The bit pattern.</returns>
    internal static ulong ToBits(object value, Type enumType) =>
        IsUnsigned(Enum.GetUnderlyingType(enumType))
            ? Convert.ToUInt64(value)
            : unchecked((ulong)Convert.ToInt64(value));

    static bool IsUnsigned(Type type) =>
        type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
}
