// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter.when_serializing;

public class and_deserializing_with_camel_case_naming_policy : given.a_set_of_derived_type_families
{
    string input;
    IParent result;

    void Establish()
    {
        options.PropertyNamingPolicy = new CamelCaseNamingPolicy().JsonPropertyNamingPolicy;
        input = JsonSerializer.Serialize<IParent>(new Parent(new Child("nested")), options);
    }

    void Because() => result = JsonSerializer.Deserialize<IParent>(input, options)!;

    [Fact] void should_preserve_the_nested_value_on_round_trip() => ((Child)((Parent)result).Child!).SomeValue.ShouldEqual("nested");
}
