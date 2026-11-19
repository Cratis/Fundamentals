// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Cratis.Concepts;

namespace Cratis.Fundamentals.AOT.Specs.for_ConceptFactory;

public class when_creating_a_concept_instance : Specification
{
    AotNameConcept _result;

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(AotNameConcept))]
    void Because() => _result = (AotNameConcept)ConceptFactory.CreateConceptInstance(typeof(AotNameConcept), "Ada");

    [Fact] void should_hold_the_value() => _result.Value.ShouldEqual("Ada");
    [Fact] void should_read_the_value_as_an_object() => _result.GetConceptValue().ShouldEqual("Ada");
}
