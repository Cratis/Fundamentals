// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_bool_to_json_value_and_back : Specification
{
    bool input;
    bool result;

    void Establish() => input = true;

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = jsonValue!.GetValue<bool>();
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
