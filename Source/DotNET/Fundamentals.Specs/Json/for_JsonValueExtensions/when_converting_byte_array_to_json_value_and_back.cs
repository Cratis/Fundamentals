// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_byte_array_to_json_value_and_back : Specification
{
    byte[] input;
    System.Text.Json.Nodes.JsonValue jsonValue;
    byte[] result;

    void Establish() => input = [1, 2, 3, 255, 0];

    void Because()
    {
        jsonValue = input.ToJsonValue()!;
        result = (byte[])jsonValue.ToTargetTypeValue(typeof(byte[]))!;
    }

    [Fact] void should_serialize_as_a_base64_string() => jsonValue.ToString().Trim('"').ShouldEqual(Convert.ToBase64String(input));
    [Fact] void should_return_same_value_after_round_tripping() => result.ShouldContainOnly(input);
}
