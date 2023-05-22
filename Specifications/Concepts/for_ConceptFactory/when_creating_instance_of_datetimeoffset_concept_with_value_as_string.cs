// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Aksio.Concepts.for_ConceptFactory;

public class when_creating_instance_of_datetimeoffset_concept_with_value_as_string : Specification
{
    DateTimeOffsetConcept result;
    string now;

    void Establish() => now = DateTimeOffset.Now.ToString();

    void Because() => result = ConceptFactory.CreateConceptInstance(typeof(DateTimeOffsetConcept), now) as DateTimeOffsetConcept;

    [Fact] void should_be_the_value_of_the_datetime() => result.Value.ToString().ShouldEqual(now);
}
