// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using TypesCollection = Cratis.Types.Types;

namespace Cratis.Concepts.for_TypesExtensions.when_registering_type_converters_for_concepts_from_types;

public class with_concept_type_without_type_converter_attribute : Specification
{
    TypesCollection types;
    ITypes result;
    Type converter_type;

    void Establish()
    {
        types = new TypesCollection([new given.assembly_provider_with_defined_types([typeof(IntConceptWithoutTypeConverterAttribute)])]);
    }

    void Because()
    {
        result = types.RegisterTypeConvertersForConcepts();
        converter_type = TypeDescriptor.GetConverter(typeof(IntConceptWithoutTypeConverterAttribute)).GetType();
    }

    [Fact] void should_return_the_types_instance() => result.ShouldEqual(types);
    [Fact] void should_add_the_concept_type_converter() => converter_type.ShouldEqual(typeof(ConceptAsTypeConverter<IntConceptWithoutTypeConverterAttribute, int>));

    record IntConceptWithoutTypeConverterAttribute(int Value) : ConceptAs<int>(Value);
}
