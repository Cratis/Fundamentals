// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Text.Json;

namespace Cratis.Json.for_ConceptAsJsonConverter;

public class when_writing_many_guid_concepts_to_json : Specification
{
    const int iterations = 1000;
    const long max_bytes_per_write = 220;

    ConceptAsJsonConverter<GuidConcept> converter;
    ArrayBufferWriter<byte> buffer;
    GuidConcept input;
    long allocated_per_write;
    string result;

    void Establish()
    {
        converter = new();
        buffer = new();
        input = new(Guid.NewGuid());
    }

    void Because()
    {
        for (var i = 0; i < 100; i++) Write();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < iterations; i++) Write();
        allocated_per_write = (GC.GetAllocatedBytesForCurrentThread() - before) / iterations;

        result = System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    [Fact] void should_write_the_guid_as_a_string() => result.ShouldEqual($"\"{input.Value}\"");
    [Fact] void should_not_allocate_reflection_data_per_write() => allocated_per_write.ShouldBeLessThan(max_bytes_per_write);

    void Write()
    {
        buffer.ResetWrittenCount();
        using var writer = new Utf8JsonWriter(buffer);
        converter.Write(writer, input, default);
    }
}
