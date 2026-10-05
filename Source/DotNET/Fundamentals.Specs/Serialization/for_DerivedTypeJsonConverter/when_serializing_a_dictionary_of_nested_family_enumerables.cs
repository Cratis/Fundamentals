// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_dictionary_of_nested_family_enumerables : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithChildren(new Dictionary<string, IEnumerable<IChild>> { ["nested"] = new List<Child> { new("value") } });

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_write_the_family_identifier_and_names() => result.ShouldEqual("{\"children\":{\"nested\":[{\"someValue\":\"value\",\"_derivedTypeId\":\"child\"}]},\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithChildren(IDictionary<string, IEnumerable<IChild>> Children) : IParent;
}
