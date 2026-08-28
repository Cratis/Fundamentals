// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Json.for_ConceptAsJsonConverter;

/// <summary>
/// A concept value type the converter's number branch has no case for must fail loudly rather than silently
/// deserializing to null - the underlying value type for CharConcept (char) is not one it recognizes.
/// </summary>
public class when_converting_a_number_to_an_unsupported_concept_value_type : Specification
{
    ConceptAsJsonConverter<CharConcept> converter;
    Exception result;

    void Establish() => converter = new();

    void Because()
    {
        const string fullJson = "{ \"prop\": 65 }";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(fullJson).AsSpan());
        reader.Read();  // Start object
        reader.Read();  // Property
        reader.Read();  // Value
        try
        {
            converter.Read(ref reader, typeof(CharConcept), default);
        }
        catch (Exception ex)
        {
            result = ex;
        }
    }

    [Fact] void should_throw_unsupported_concept_value_type() => result.ShouldBeOfExactType<UnsupportedConceptValueType>();
}
