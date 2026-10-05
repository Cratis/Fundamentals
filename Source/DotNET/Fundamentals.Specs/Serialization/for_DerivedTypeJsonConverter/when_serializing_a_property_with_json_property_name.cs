// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_property_with_json_property_name : given.derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithRenamedValue("value");

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_keep_the_immediate_camel_case_property_name() => result.ShouldEqual("{\"someValue\":\"value\",\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithRenamedValue([property: JsonPropertyName("renamed")] string SomeValue) : IParent;
}
