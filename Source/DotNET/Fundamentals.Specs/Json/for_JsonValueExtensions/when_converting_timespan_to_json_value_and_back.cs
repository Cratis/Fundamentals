// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_timespan_to_json_value_and_back : Specification
{
    TimeSpan input;
    TimeSpan result;

    void Establish() => input = TimeSpan.FromHours(2.5);

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (TimeSpan)jsonValue!.ToTargetTypeValue(typeof(TimeSpan))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
