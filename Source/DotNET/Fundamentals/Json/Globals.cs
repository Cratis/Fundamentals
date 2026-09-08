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
    public static void Configure(IDerivedTypes derivedTypes) =>
        LazyInitializer.EnsureInitialized(ref _jsonSerializerOptions, () => Build(derivedTypes));

    /// <summary>
    /// Builds the options in full before they are published: a reader that sees the field non-null may
    /// serialize with it at once, and System.Text.Json freezes the options on first use, so a converter
    /// added afterwards throws.
    /// </summary>
    /// <param name="derivedTypes"><see cref="IDerivedTypes"/> to add the derived-type converter for.</param>
    /// <returns>The fully configured options.</returns>
    static JsonSerializerOptions Build(IDerivedTypes derivedTypes)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = AcronymFriendlyJsonCamelCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new ComplexKeyDictionaryJsonConverterFactory(),
                new EnumConverterFactory(),
                new EnumerableConceptAsJsonConverterFactory(),
                new ConceptAsJsonConverterFactory(),
                new DateOnlyJsonConverter(),
                new TimeOnlyJsonConverter(),
                new TypeJsonConverter(),
                new UriJsonConverter(),
                new PointJsonConverter(),
                new LineStringJsonConverter(),
                new PolygonJsonConverter(),
                new EnumerableModelWithIdToConceptOrPrimitiveEnumerableConverterFactory()
            }
        };

        if (derivedTypes is not null)
        {
            options.Converters.Add(new DerivedTypeJsonConverterFactory(derivedTypes));
        }

        return options;
    }
}