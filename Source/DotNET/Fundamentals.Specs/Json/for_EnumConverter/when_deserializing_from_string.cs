// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_deserializing_from_string : given.configured_serialization_options
{
    MyEnum _result;
    string _input;

    void Establish() => _input = $"\"{nameof(MyEnum.Three)}\"";

    void Because() => _result = JsonSerializer.Deserialize<MyEnum>(_input, _options);

    [Fact] void should_deserialize_correctly() => _result.ShouldEqual(MyEnum.Three);
}
