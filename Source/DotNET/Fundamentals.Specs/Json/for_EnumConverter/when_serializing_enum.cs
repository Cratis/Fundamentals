// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_serializing_enum : given.configured_serialization_options
{
    MyEnum _value;
    string _result;

    void Establish() => _value = MyEnum.Two;

    void Because() => _result = JsonSerializer.Serialize(_value, _options);

    [Fact] void should_serialize_as_number() => _result.ShouldEqual(MyEnum.Two.ToString("D"));
}
