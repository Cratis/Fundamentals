// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_byte_to_json_value_and_back : Specification
{
    byte input;
    byte result;

    void Establish() => input = 42;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (byte)jsonValue!.ToTargetTypeValue(typeof(byte))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
