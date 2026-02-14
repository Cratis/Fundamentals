// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptAsTypeConverter.when_converting_to;

public class with_non_concept_value : given.a_concept_as_type_converter
{
    Exception exception;

    void Because() => exception = Catch.Exception(() => converter.ConvertTo(42, typeof(int)));

    [Fact] void should_throw_type_is_not_a_concept() => exception.ShouldBeOfExactType<TypeIsNotAConcept>();
}
