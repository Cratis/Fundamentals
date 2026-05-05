// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Concepts;

namespace Cratis.Json.for_ComplexKeyDictionaryJsonConverterFactory;

public class when_serializing_and_deserializing_dictionary_with_concept_as_string_key : Specification
{
    JsonSerializerOptions _options;
    IDictionary<PropertyPath, int> _dictionary;
    string _json;
    IDictionary<PropertyPath, int> _deserialized;

    void Establish()
    {
        _options = new JsonSerializerOptions
        {
            Converters =
            {
                new ComplexKeyDictionaryJsonConverterFactory(),
                new EnumerableConceptAsJsonConverterFactory(),
                new ConceptAsJsonConverterFactory()
            }
        };

        _dictionary = new Dictionary<PropertyPath, int>
        {
            [new("spec.name")] = 1,
            [new("spec.version")] = 2
        };
    }

    void Because()
    {
        _json = JsonSerializer.Serialize(_dictionary, _options);
        _deserialized = JsonSerializer.Deserialize<IDictionary<PropertyPath, int>>(_json, _options)!;
    }

    [Fact] void should_deserialize_all_items() => _deserialized.Count.ShouldEqual(2);
    [Fact] void should_deserialize_first_item() => _deserialized[new PropertyPath("spec.name")].ShouldEqual(1);
    [Fact] void should_deserialize_second_item() => _deserialized[new PropertyPath("spec.version")].ShouldEqual(2);

    public record PropertyPath(string Value) : ConceptAs<string>(Value);
}
