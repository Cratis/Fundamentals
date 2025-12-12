// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_float_to_json_value_and_back : Specification
{
    float input;
    float result;

    void Establish() => input = 42.5f;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (float)jsonValue!.ToTargetTypeValue(typeof(float))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
