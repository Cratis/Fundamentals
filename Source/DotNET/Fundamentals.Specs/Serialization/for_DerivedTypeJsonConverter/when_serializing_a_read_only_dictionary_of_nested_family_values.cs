// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_read_only_dictionary_of_nested_family_values : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithChildren(new Dictionary<string, IChild> { ["nested"] = new Child("value") });

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_write_the_family_identifier_and_names() => result.ShouldEqual("{\"children\":{\"nested\":{\"someValue\":\"value\",\"_derivedTypeId\":\"child\"}},\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithChildren(IReadOnlyDictionary<string, IChild> Children) : IParent;
}
