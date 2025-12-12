// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_datetime_to_json_value_and_back : Specification
{
    DateTime input;
    DateTime result;

    void Establish() => input = DateTime.UtcNow;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (DateTime)jsonValue!.ToTargetTypeValue(typeof(DateTime))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
