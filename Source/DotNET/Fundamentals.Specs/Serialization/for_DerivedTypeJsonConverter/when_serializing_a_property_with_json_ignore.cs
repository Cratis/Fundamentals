// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_property_with_json_ignore : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithIgnoredValue("ignored-value-text");

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_only_write_the_derived_type_id() => result.ShouldEqual("{\"_derivedTypeId\":\"parent\"}");
    [Fact] void should_not_contain_the_ignored_value() => result.ShouldNotContain("ignored-value-text");

    [DerivedType("parent")]
    record ParentWithIgnoredValue([property: JsonIgnore] string SomeValue) : IParent;
}
