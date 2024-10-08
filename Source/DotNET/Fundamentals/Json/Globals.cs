// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Serialization;

namespace Cratis.Json;

/// <summary>
/// Represents global configuration for JSON serialization.
/// </summary>
public static class Globals
{
    static JsonSerializerOptions? _jsonSerializerOptions;

    /// <summary>
    /// Gets the global <see cref="JsonSerializerOptions"/> - it can be null if not initialized.
    /// </summary>
    public static JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            if (_jsonSerializerOptions is null)
            {
                Configure(DerivedTypes.Instance);
            }

            return _jsonSerializerOptions!;
        }
    }

    /// <summary>
    /// Configure the globals.
    /// </summary>
    /// <param name="derivedTypes"><see cref="IDerivedTypes"/>.</param>
    public static void Configure(IDerivedTypes derivedTypes)
    {
        if (_jsonSerializerOptions is not null)
        {
            return;
        }

        _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = AcronymFriendlyJsonCamelCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new EnumConverterFactory(),
                new EnumerableConceptAsJsonConverterFactory(),
                new ConceptAsJsonConverterFactory(),
                new DateOnlyJsonConverter(),
                new TimeOnlyJsonConverter(),
                new TypeJsonConverter(),
                new UriJsonConverter(),
                new EnumerableModelWithIdToConceptOrPrimitiveEnumerableConverterFactory()
            }
        };

        if (derivedTypes is not null)
        {
            _jsonSerializerOptions.Converters.Add(new DerivedTypeJsonConverterFactory(derivedTypes));
        }
    }
}