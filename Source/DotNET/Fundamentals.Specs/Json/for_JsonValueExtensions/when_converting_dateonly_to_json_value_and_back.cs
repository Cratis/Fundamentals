// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_dateonly_to_json_value_and_back : Specification
{
    DateOnly input;
    DateOnly result;

    void Establish() => input = DateOnly.FromDateTime(DateTime.UtcNow);

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (DateOnly)jsonValue!.ToTargetTypeValue(typeof(DateOnly))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
