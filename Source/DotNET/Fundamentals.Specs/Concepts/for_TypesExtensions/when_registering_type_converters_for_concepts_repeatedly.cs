// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Cratis.Concepts.for_TypesExtensions;

public class when_registering_type_converters_for_concepts_repeatedly : Specification
{
    TypeDescriptionProvider _providerAfterTheFirstCall;
    TypeDescriptionProvider _providerAfterTheSecondCall;
    TypeConverter _converter;

    void Because()
    {
        typeof(when_registering_type_converters_for_concepts_repeatedly).Assembly.RegisterTypeConvertersForConcepts();
        _providerAfterTheFirstCall = TypeDescriptor.GetProvider(typeof(a_concept));

        typeof(when_registering_type_converters_for_concepts_repeatedly).Assembly.RegisterTypeConvertersForConcepts();
        _providerAfterTheSecondCall = TypeDescriptor.GetProvider(typeof(a_concept));

        _converter = TypeDescriptor.GetConverter(typeof(a_concept));
    }

    [Fact] void should_register_the_concept_converter() => _converter.ShouldBeOfExactType<ConceptAsTypeConverter<a_concept, int>>();
    [Fact] void should_not_stack_another_provider_on_the_second_call() => _providerAfterTheSecondCall.ShouldBeSame(_providerAfterTheFirstCall);

    public sealed record a_concept(int Value) : ConceptAs<int>(Value);
}
