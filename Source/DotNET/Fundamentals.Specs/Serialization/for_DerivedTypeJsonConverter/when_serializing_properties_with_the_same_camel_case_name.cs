// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_properties_with_the_same_camel_case_name : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithDuplicateNames("first", "last");

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_write_the_key_once_with_the_last_value() => result.ShouldEqual("{\"foo\":\"last\",\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithDuplicateNames(string Foo, string foo) : IParent;
}
