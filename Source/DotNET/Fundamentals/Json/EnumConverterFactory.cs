// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverterFactory"/> for converting enums.
/// </summary>
public class EnumConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Uses MakeGenericType on EnumConverter<T>; enum types are always preserved.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "EnumConverter<T> has a public parameterless constructor that is always preserved.")]
    [UnconditionalSuppressMessage("Trimming", "IL2071", Justification = "EnumConverter<T> has a public parameterless constructor that is always preserved.")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Create the appropriate converter for the enum type.
        var converterType = typeof(EnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}