// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Cratis.Concepts.for_TypesExtensions.when_registering_type_converters_for_concepts_from_assembly;

public class with_concept_type_with_existing_type_converter_attribute : Specification
{
    Type converter_type;

    void Because()
    {
        typeof(IntConceptWithExistingTypeConverterAttribute).Assembly.RegisterTypeConvertersForConcepts();
        converter_type = TypeDescriptor.GetConverter(typeof(IntConceptWithExistingTypeConverterAttribute)).GetType();
    }

    [Fact] void should_keep_the_existing_type_converter() => converter_type.ShouldEqual(typeof(ExistingTypeConverter));

    [TypeConverter(typeof(ExistingTypeConverter))]
    record IntConceptWithExistingTypeConverterAttribute(int Value) : ConceptAs<int>(Value);

    class ExistingTypeConverter : TypeConverter;
}
