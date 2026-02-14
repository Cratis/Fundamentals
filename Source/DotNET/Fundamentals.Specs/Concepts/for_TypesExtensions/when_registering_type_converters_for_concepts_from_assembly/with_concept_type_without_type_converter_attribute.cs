// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Cratis.Concepts.for_TypesExtensions.when_registering_type_converters_for_concepts_from_assembly;

public class with_concept_type_without_type_converter_attribute : Specification
{
    Type converter_type;

    void Because()
    {
        typeof(IntConceptWithoutTypeConverterAttribute).Assembly.RegisterTypeConvertersForConcepts();
        converter_type = TypeDescriptor.GetConverter(typeof(IntConceptWithoutTypeConverterAttribute)).GetType();
    }

    [Fact] void should_add_the_concept_type_converter() => converter_type.ShouldEqual(typeof(ConceptAsTypeConverter<IntConceptWithoutTypeConverterAttribute, int>));

    record IntConceptWithoutTypeConverterAttribute(int Value) : ConceptAs<int>(Value);
}
