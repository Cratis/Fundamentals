// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_decimal_to_json_value_and_back : Specification
{
    decimal input;
    decimal result;

    void Establish() => input = 42.5m;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (decimal)jsonValue!.ToTargetTypeValue(typeof(decimal))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
