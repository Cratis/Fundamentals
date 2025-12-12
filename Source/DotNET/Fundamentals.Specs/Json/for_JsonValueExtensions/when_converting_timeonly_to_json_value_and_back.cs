// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_timeonly_to_json_value_and_back : Specification
{
    TimeOnly input;
    TimeOnly result;

    void Establish() => input = TimeOnly.FromDateTime(DateTime.UtcNow);

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (TimeOnly)jsonValue!.ToTargetTypeValue(typeof(TimeOnly))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
