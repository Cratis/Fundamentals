// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_an_object_declared_family_collection : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithChildren(new List<Child> { new("nested") });

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_keep_the_runtime_collection_type() => result.ShouldEqual("{\"children\":[{\"SomeValue\":\"nested\"}],\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithChildren(object Children) : IParent;
}
