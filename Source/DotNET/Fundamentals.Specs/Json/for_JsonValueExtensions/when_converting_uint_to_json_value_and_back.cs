// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_uint_to_json_value_and_back : Specification
{
    uint input;
    uint result;

    void Establish() => input = 42;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (uint)jsonValue!.ToTargetTypeValue(typeof(uint))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
