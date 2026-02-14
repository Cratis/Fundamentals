// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptAsTypeConverter.when_converting_from;

public class with_null_value : given.a_concept_as_type_converter
{
    Exception exception;

    void Because() => exception = Catch.Exception(() => converter.ConvertFrom(null));

    [Fact] void should_throw_an_argument_null_exception() => exception.ShouldBeOfExactType<ArgumentNullException>();
}
