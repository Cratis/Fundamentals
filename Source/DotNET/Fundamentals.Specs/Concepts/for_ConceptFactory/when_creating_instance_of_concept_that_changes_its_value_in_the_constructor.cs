// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptFactory;

public class when_creating_instance_of_concept_that_changes_its_value_in_the_constructor : Specification
{
    const string input = "  padded  ";

    TrimmingConcept result;

    void Because() => result = ConceptFactory.CreateConceptInstance(typeof(TrimmingConcept), input) as TrimmingConcept;

    [Fact] void should_hold_the_value_as_given() => result.Value.ShouldEqual(input);
}
