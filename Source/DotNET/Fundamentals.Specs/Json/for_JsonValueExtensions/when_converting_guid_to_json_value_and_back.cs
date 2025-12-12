// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_guid_to_json_value_and_back : Specification
{
    Guid input;
    Guid result;

    void Establish() => input = Guid.NewGuid();

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = (Guid)jsonValue!.ToTargetTypeValue(typeof(Guid))!;
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
