// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Concepts;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverterFactory"/> for providing <see cref="ConceptAsJsonConverter{T}"/> for concept types.
/// </summary>
public class ConceptAsJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsConcept();

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Uses MakeGenericType on ConceptAsJsonConverter<T> which is registered at startup.")]
    [UnconditionalSuppressMessage("Trimming", "IL2071", Justification = "ConceptAsJsonConverter<T> has a public parameterless constructor that is always preserved.")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ConceptAsJsonConverter<>).MakeGenericType(typeToConvert);
        return (Activator.CreateInstance(converterType) as JsonConverter)!;
    }
}
