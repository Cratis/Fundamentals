// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_deserializing_from_unknown_string : given.configured_serialization_options
{
    Exception _result;
    string _input;

    void Establish() => _input = $"{Guid.NewGuid()}";

    void Because() => _result = Catch.Exception(() => JsonSerializer.Deserialize<MyEnum>(_input, _options));

    [Fact] void should_throw_json_exception() => _result.ShouldBeOfExactType<JsonException>();
}
