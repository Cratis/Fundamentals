// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Json.for_ConceptAsJsonConverter;

public class when_reading_many_string_concepts_from_json : Specification
{
    const int iterations = 1000;
    const long max_bytes_per_read = 250;

    ConceptAsJsonConverter<StringConcept> converter;
    byte[] json;
    long allocated_per_read;
    StringConcept last;

    void Establish()
    {
        converter = new();
        json = Encoding.UTF8.GetBytes("\"some value\"");
    }

    void Because()
    {
        for (var i = 0; i < 100; i++) Read();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < iterations; i++) Read();
        allocated_per_read = (GC.GetAllocatedBytesForCurrentThread() - before) / iterations;
    }

    [Fact] void should_read_the_value() => last.Value.ShouldEqual("some value");
    [Fact] void should_not_allocate_reflection_data_per_read() => allocated_per_read.ShouldBeLessThan(max_bytes_per_read);

    void Read()
    {
        var reader = new Utf8JsonReader(json);
        reader.Read();
        last = converter.Read(ref reader, typeof(StringConcept), default);
    }
}
