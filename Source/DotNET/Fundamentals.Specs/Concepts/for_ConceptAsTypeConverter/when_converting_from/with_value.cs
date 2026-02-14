// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptAsTypeConverter.when_converting_from;

public class with_value : given.a_concept_as_type_converter
{
    object result;

    void Because() => result = converter.ConvertFrom(42);

    [Fact] void should_return_a_concept() => result.ShouldBeOfExactType<TypeConverterConcept>();
    [Fact] void should_hold_the_value() => ((TypeConverterConcept)result).Value.ShouldEqual(42);
}
