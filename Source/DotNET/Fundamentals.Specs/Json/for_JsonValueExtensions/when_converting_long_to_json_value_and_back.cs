// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_long_to_json_value_and_back : Specification
{
    long input;
    long result;

    void Establish() => input = 42L;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (long)jsonValue!.ToTargetTypeValue(typeof(long))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
