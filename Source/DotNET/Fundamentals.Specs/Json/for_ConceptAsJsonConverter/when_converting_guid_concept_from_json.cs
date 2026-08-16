// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Json.for_ConceptAsJsonConverter;

public class when_converting_guid_concept_from_json : given.converter_for_converting_from_json<GuidConcept, Guid>
{
    protected override Guid InputValue => Guid.NewGuid();

    protected override string FormattedInput => $"\"{input}\"";

    void Because()
    {
        var fullJson = $"{{ \"prop\": {FormattedInput} }}";

        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(fullJson).AsSpan());
        reader.Read();  // Start object
        reader.Read();  // Property
        reader.Read();  // Value
        result = converter.Read(ref reader, ConceptType, default);
    }

    [Fact] void should_convert_to_correct_guid() => ShouldConvertToCorrectConcept();
}
