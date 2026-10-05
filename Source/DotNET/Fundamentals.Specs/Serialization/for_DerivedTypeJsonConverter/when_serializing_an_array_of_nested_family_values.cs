// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_an_array_of_nested_family_values : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithChildren([new Child("nested")]);

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_write_the_array_element_type_identifier() => JsonDocument.Parse(result).RootElement.GetProperty("children")[0].GetProperty("_derivedTypeId").GetString().ShouldEqual("child");

    [DerivedType("parent")]
    record ParentWithChildren(IChild[] Children) : IParent;
}
