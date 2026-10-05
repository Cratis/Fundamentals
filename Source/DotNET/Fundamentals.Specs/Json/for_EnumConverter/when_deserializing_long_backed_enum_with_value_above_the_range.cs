// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_deserializing_long_backed_enum_with_value_above_the_range : given.configured_serialization_options
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => JsonSerializer.Deserialize<given.LongBackedEnum>("9223372036854775808", _options));

    [Fact] void should_throw_json_exception() => _result.ShouldBeOfExactType<JsonException>();
    [Fact] void should_name_the_enum() => _result.Message.ShouldContain(nameof(given.LongBackedEnum));
    [Fact] void should_name_the_backing_type() => _result.Message.ShouldContain("System.Int64");
    [Fact] void should_include_the_raw_value() => _result.Message.ShouldContain("9223372036854775808");
}
