// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Json.for_EnumerableConceptAsJsonConverterFactory;

public class when_asking_can_convert_for_dictionary_with_concept_key : Specification
{
    bool _result;

    void Because() => _result = new EnumerableConceptAsJsonConverterFactory().CanConvert(typeof(IDictionary<PropertyPath, int>));

    [Fact] void should_not_be_able_to_convert() => _result.ShouldBeFalse();

    public record PropertyPath(string Value) : ConceptAs<string>(Value);
}
