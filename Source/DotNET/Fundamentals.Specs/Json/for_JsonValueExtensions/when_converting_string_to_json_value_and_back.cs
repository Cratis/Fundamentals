// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_JsonValueExtensions;

public class when_converting_string_to_json_value_and_back : Specification
{
    string input;
    string result;

    void Establish() => input = "Hello World";

    void Because()
    {
        var jsonValue = input.ToJsonValue();
        result = jsonValue!.GetValue<string>();
    }

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
