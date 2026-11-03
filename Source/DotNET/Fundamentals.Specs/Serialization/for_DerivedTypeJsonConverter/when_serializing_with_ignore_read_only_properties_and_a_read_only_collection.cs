// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_with_ignore_read_only_properties_and_a_read_only_collection : given.a_set_of_derived_type_families
{
    string result;

    void Establish() => options.IgnoreReadOnlyProperties = true;

    void Because() => result = JsonSerializer.Serialize<IParent>(new ParentWithItems(), options);

    [Fact] void should_write_the_collection() => result.ShouldEqual("{\"items\":[\"a\"],\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    class ParentWithItems : IParent
    {
        public List<string> Items { get; } = ["a"];

        public string Dropped => "dropped";
    }
}
