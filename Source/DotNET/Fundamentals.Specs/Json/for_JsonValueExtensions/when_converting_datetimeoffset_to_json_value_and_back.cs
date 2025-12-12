// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_datetimeoffset_to_json_value_and_back : Specification
{
    DateTimeOffset input;
    DateTimeOffset result;

    void Establish() => input = DateTimeOffset.UtcNow;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (DateTimeOffset)jsonValue!.ToTargetTypeValue(typeof(DateTimeOffset))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
