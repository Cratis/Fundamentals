// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Cratis.Concepts.for_TypesExtensions.when_registering_type_converters_for_concepts_from_assembly;

public class with_non_concept_type : Specification
{
    Type converter_type;

    void Because()
    {
        typeof(NonConceptType).Assembly.RegisterTypeConvertersForConcepts();
        converter_type = TypeDescriptor.GetConverter(typeof(NonConceptType)).GetType();
    }

    [Fact] void should_not_add_a_concept_type_converter() => converter_type.ShouldEqual(typeof(TypeConverter));

    class NonConceptType;
}
