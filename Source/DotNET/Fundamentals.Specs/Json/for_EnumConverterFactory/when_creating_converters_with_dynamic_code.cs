// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Json.for_EnumConverterFactory;

public class when_creating_converters_with_dynamic_code : Specification
{
    EnumConverterFactory _factory;
    JsonSerializerOptions _options;
    JsonConverter _enumConverter;

    void Establish()
    {
        _factory = new EnumConverterFactory();
        _options = new JsonSerializerOptions();
    }

    void Because()
    {
        _enumConverter = _factory.CreateConverter(typeof(DayOfWeek), _options);
    }

    [Fact] void should_create_the_existing_converter() => _enumConverter.ShouldBeOfExactType<EnumConverter<DayOfWeek>>();
    [Fact] void should_accept_enums() => _factory.CanConvert(typeof(DayOfWeek)).ShouldBeTrue();
    [Fact] void should_leave_nullable_enum_handling_to_the_serializer() => _factory.CanConvert(typeof(DayOfWeek?)).ShouldBeFalse();
    [Fact] void should_reject_other_types() => _factory.CanConvert(typeof(string)).ShouldBeFalse();
}
