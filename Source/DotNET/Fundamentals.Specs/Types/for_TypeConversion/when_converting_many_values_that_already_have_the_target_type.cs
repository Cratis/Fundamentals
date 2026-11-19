// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeConversion;

public class when_converting_many_values_that_already_have_the_target_type : Specification
{
    const long iterations = 1000;

    object input;
    object result;
    long allocated;

    void Establish() => input = 42;

    void Because()
    {
        for (var i = 0; i < 100; i++) TypeConversion.Convert(typeof(int), input);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < iterations; i++) result = TypeConversion.Convert(typeof(int), input);
        allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact] void should_return_the_same_value() => result.ShouldEqual(input);
    [Fact] void should_not_allocate() => allocated.ShouldBeLessThan(iterations);
}
