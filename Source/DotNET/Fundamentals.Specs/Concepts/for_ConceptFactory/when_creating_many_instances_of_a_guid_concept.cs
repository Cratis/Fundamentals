// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptFactory;

public class when_creating_many_instances_of_a_guid_concept : Specification
{
    const int iterations = 1000;
    const long max_bytes_per_instance = 250;

    long allocated_per_instance;
    Guid value;

    void Establish() => value = Guid.NewGuid();

    void Because()
    {
        for (var i = 0; i < 100; i++) ConceptFactory.CreateConceptInstance(typeof(GuidConcept), value);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < iterations; i++) ConceptFactory.CreateConceptInstance(typeof(GuidConcept), value);
        allocated_per_instance = (GC.GetAllocatedBytesForCurrentThread() - before) / iterations;
    }

    [Fact] void should_not_allocate_reflection_data_per_instance() => allocated_per_instance.ShouldBeLessThan(max_bytes_per_instance);
}
