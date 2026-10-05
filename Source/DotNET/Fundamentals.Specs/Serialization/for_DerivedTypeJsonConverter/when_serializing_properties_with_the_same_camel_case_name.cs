// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_properties_with_the_same_camel_case_name : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithDuplicateNames("first", "second", "last", "final");

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_keep_each_keys_first_position_with_its_last_value() => result.ShouldEqual("{\"foo\":\"last\",\"bar\":\"final\",\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithDuplicateNames(string Foo, string Bar, string foo, string bar) : IParent;
}
