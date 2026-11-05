// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_nested_family_with_json_ignore : given.a_set_of_derived_type_families
{
    string result;
    IParent deserialized;

    void Because()
    {
        result = JsonSerializer.Serialize<IParent>(new Parent(new IgnoringChild("secret-text", "visible")), options);
        deserialized = JsonSerializer.Deserialize<IParent>("{\"_derivedTypeId\":\"parent\"}", options);
    }

    [Fact] void should_omit_ignored_value_in_nested_child() => result.ShouldEqual("{\"child\":{\"visible\":\"visible\",\"_derivedTypeId\":\"child\"},\"_derivedTypeId\":\"parent\"}");
    [Fact] void should_not_contain_the_ignored_value() => result.ShouldNotContain("secret-text");
    [Fact] void should_still_deserialize_the_family() => deserialized.ShouldBeOfExactType<Parent>();

    [DerivedType("child")]
    record IgnoringChild([property: JsonIgnore] string Secret, string Visible) : IChild;
}
