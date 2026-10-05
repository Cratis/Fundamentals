// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_with_camel_case_naming_policy : given.derived_type_families
{
    IParent input;
    string result;

    void Establish()
    {
        options.PropertyNamingPolicy = new CamelCaseNamingPolicy().JsonPropertyNamingPolicy;
        input = new Parent(new Child("nested"));
    }

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_use_immediate_camel_case_names_for_each_family() => result.ShouldEqual("{\"child\":{\"someValue\":\"nested\",\"_derivedTypeId\":\"child\"},\"_derivedTypeId\":\"parent\"}");
    [Fact] void should_preserve_the_nested_value_on_round_trip() => ((Child)((Parent)JsonSerializer.Deserialize<IParent>(result, options)!).Child!).SomeValue.ShouldEqual("nested");
}
