// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_ComplexKeyDictionaryJsonConverterFactory;

public class when_serializing_and_deserializing_dictionary_with_complex_key : Specification
{
    JsonSerializerOptions _options;
    IDictionary<complex_key, complex_value> _dictionary;
    string _json;
    IDictionary<complex_key, complex_value> _deserialized;

    void Establish()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new ComplexKeyDictionaryJsonConverterFactory() }
        };

        _dictionary = new Dictionary<complex_key, complex_value>
        {
            [new complex_key(Guid.Parse("e094c582-9d91-4df0-b795-8d39fce089da"), "tenant-a")] = new complex_value(42),
            [new complex_key(Guid.Parse("486be95f-c860-4acd-b31e-39273b57bfc7"), "tenant-b")] = new complex_value(43)
        };
    }

    void Because()
    {
        _json = JsonSerializer.Serialize(_dictionary, _options);
        _deserialized = JsonSerializer.Deserialize<IDictionary<complex_key, complex_value>>(_json, _options)!;
    }

    [Fact] void should_serialize_key_as_compact_json() => _json.ShouldContain("\\u0022id\\u0022:\\u0022e094c582-9d91-4df0-b795-8d39fce089da\\u0022,\\u0022name\\u0022:\\u0022tenant-a\\u0022");
    [Fact] void should_deserialize_two_items() => _deserialized.Count.ShouldEqual(2);
    [Fact] void should_deserialize_first_key() => _deserialized.Keys.ShouldContain(new complex_key(Guid.Parse("e094c582-9d91-4df0-b795-8d39fce089da"), "tenant-a"));
    [Fact] void should_deserialize_first_value() => _deserialized[new complex_key(Guid.Parse("e094c582-9d91-4df0-b795-8d39fce089da"), "tenant-a")].number.ShouldEqual(42);

    public record complex_key(Guid id, string name);

    public record complex_value(int number);
}