// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Json.for_ConceptAsJsonConverter;

public class when_converting_byte_backed_enum_concept_with_a_value_above_the_range_from_json : Specification
{
    ConceptAsJsonConverter<ByteBackedEnumConcept> converter;
    Exception result;

    void Establish() => converter = new();

    void Because()
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("{ \"prop\": 256 }").AsSpan());
        reader.Read();  // Start object
        reader.Read();  // Property
        reader.Read();  // Value
        try
        {
            converter.Read(ref reader, typeof(ByteBackedEnumConcept), default);
        }
        catch (Exception ex)
        {
            result = ex;
        }
    }

    [Fact] void should_throw_a_json_exception() => result.ShouldBeOfExactType<JsonException>();
}
