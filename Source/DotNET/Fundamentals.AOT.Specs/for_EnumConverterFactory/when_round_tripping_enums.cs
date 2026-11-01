// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Json;

namespace Cratis.Fundamentals.AOT.Specs.for_EnumConverterFactory;

public class when_round_tripping_enums : Specification
{
    JsonSerializerOptions _options;
    AotJsonSerializerContext _context;
    JsonConverter _converter;

    void Establish()
    {
        _options = new JsonSerializerOptions
        {
            TypeInfoResolver = AotJsonSerializerContext.Default,
            Converters = { new EnumConverterFactory() }
        };
        _context = new AotJsonSerializerContext(_options);
    }

    void Because() => _converter = new EnumConverterFactory().CreateConverter(typeof(AotColor), _options);

    [Fact] void should_write_numbers() => JsonSerializer.Serialize(AotColor.Green, _context.AotColor).ShouldEqual("2");
    [Fact] void should_read_names_case_insensitively() => JsonSerializer.Deserialize("\"green\"", _context.AotColor).ShouldEqual(AotColor.Green);
    [Fact] void should_round_trip_nullable_values() => JsonSerializer.Deserialize(JsonSerializer.Serialize(AotColor.Red, _context.NullableAotColor), _context.NullableAotColor).ShouldEqual(AotColor.Red);
    [Fact] void should_round_trip_null() => JsonSerializer.Deserialize(JsonSerializer.Serialize(null, _context.NullableAotColor), _context.NullableAotColor).ShouldBeNull();
    [Fact] void should_write_flag_combinations() => JsonSerializer.Serialize(AotPermissions.Read | AotPermissions.Write, _context.AotPermissions).ShouldEqual("3");
    [Fact] void should_read_flag_combinations() => JsonSerializer.Deserialize("3", _context.AotPermissions).ShouldEqual(AotPermissions.Read | AotPermissions.Write);
    [Fact] void should_reject_undefined_values() => Catch.Exception(() => JsonSerializer.Deserialize("42", _context.AotColor)).ShouldBeOfExactType<JsonException>();
    [Fact] void should_create_a_typed_converter() => (_converter is JsonConverter<AotColor>).ShouldBeTrue();
    [Fact] void should_create_the_enum_converter() => _converter.GetType().Name.ShouldEqual("EnumConverter`1");
}
