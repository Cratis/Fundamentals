// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Concepts.given;

namespace Cratis.Json.for_ConceptAsJsonConverter;

/// <summary>
/// A JSON number posted for a string-backed concept is malformed input. Deserialization must fail with a
/// <see cref="JsonException"/> that carries the path of the offending property, so hosts report it as a bad request.
/// </summary>
public class when_deserializing_a_number_into_a_string_concept_property : Specification
{
    record Payload(StringConcept Email);

    JsonSerializerOptions options;
    Exception result;

    void Establish()
    {
        options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ConceptAsJsonConverterFactory());
    }

    void Because() => result = Catch.Exception(() => JsonSerializer.Deserialize<Payload>("{ \"email\": 42 }", options));

    [Fact] void should_throw_unsupported_concept_value_type() => result.ShouldBeOfExactType<UnsupportedConceptValueType>();
    [Fact] void should_be_a_json_exception() => (result is JsonException).ShouldBeTrue();
    [Fact] void should_include_the_property_path() => ((JsonException)result).Path.ShouldEqual("$.email");
}
