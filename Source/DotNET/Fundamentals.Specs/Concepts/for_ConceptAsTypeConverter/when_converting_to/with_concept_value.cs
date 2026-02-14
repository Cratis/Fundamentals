// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptAsTypeConverter.when_converting_to;

public class with_concept_value : given.a_concept_as_type_converter
{
    object? result;

    void Because() => result = converter.ConvertTo(new TypeConverterConcept(42), typeof(int));

    [Fact] void should_return_the_primitive_value() => result.ShouldEqual(42);
}
