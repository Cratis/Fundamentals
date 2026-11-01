// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverterFactory;

public class when_measuring_converter_allocations : Specification
{
    const int Iterations = 1000;
    EnumConverterFactory _factory;
    JsonSerializerOptions _options;
    long _factoryBytes;
    long _baselineBytes;

    void Establish()
    {
        _factory = new EnumConverterFactory();
        _options = new JsonSerializerOptions();
        for (var i = 0; i < Iterations; i++)
        {
            _ = _factory.CreateConverter(typeof(DayOfWeek), _options);
            _ = Activator.CreateInstance(typeof(EnumConverter<>).MakeGenericType(typeof(DayOfWeek)));
        }
    }

    void Because()
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Iterations; i++)
        {
            _ = _factory.CreateConverter(typeof(DayOfWeek), _options);
        }

        _factoryBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Iterations; i++)
        {
            _ = Activator.CreateInstance(typeof(EnumConverter<>).MakeGenericType(typeof(DayOfWeek)));
        }

        _baselineBytes = GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact] void should_add_no_allocations_to_the_existing_reflection_path() => _factoryBytes.ShouldEqual(_baselineBytes);
}
