// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_ulong_to_json_value_and_back : Specification
{
    ulong input;
    ulong result;

    void Establish() => input = 42UL;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (ulong)jsonValue!.ToTargetTypeValue(typeof(ulong))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
