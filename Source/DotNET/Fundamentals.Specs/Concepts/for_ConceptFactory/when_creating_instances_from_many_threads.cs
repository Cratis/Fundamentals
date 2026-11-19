// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptFactory;

public class when_creating_instances_from_many_threads : Specification
{
    const int threads = 16;
    const int iterations = 200;

    long[] results;

    void Because()
    {
        results = new long[threads];
        Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, thread =>
        {
            for (var i = 0; i < iterations; i++)
            {
                var concept = (MultiLevelInheritedConcept)ConceptFactory.CreateConceptInstance(typeof(MultiLevelInheritedConcept), (long)i);
                results[thread] += concept.Value;
            }
        });
    }

    [Fact] void should_create_correct_instances_on_every_thread() => results.All(_ => _ == iterations * (iterations - 1) / 2).ShouldBeTrue();
}
