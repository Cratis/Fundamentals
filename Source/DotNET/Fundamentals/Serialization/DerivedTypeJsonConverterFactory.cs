// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization;

/// <summary>
/// Represents a <see cref="JsonConverterFactory"/> for creating converters for types that are adorned with <see cref="DerivedTypeAttribute"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DerivedTypeJsonConverterFactory"/> class.
/// </remarks>
/// <param name="derivedTypes"><see cref="IDerivedTypes"/> to use for discovering correct type.</param>
public class DerivedTypeJsonConverterFactory(IDerivedTypes derivedTypes) : JsonConverterFactory
{
    readonly IDerivedTypes _derivedTypes = derivedTypes;

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => _derivedTypes.HasDerivatives(typeToConvert);

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        Activator.CreateInstance(typeof(DerivedTypeJsonConverter<>).MakeGenericType(typeToConvert), _derivedTypes) as JsonConverter;
}
