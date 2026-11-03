// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_with_ignore_read_only_properties_and_ignore_never : given.a_set_of_derived_type_families
{
    string result;

    void Establish() => options.IgnoreReadOnlyProperties = true;

    void Because() => result = JsonSerializer.Serialize<IParent>(new ReadOnlyParent(), options);

    [Fact] void should_write_the_property_marked_with_ignore_never() => result.ShouldEqual("{\"kept\":\"kept\",\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    class ReadOnlyParent : IParent
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Kept => "kept";

        public string Dropped => "dropped";
    }
}
