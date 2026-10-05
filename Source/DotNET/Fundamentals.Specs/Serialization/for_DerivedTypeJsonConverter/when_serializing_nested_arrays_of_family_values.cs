// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_nested_arrays_of_family_values : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    // Keep concrete arrays at both levels so the spec exercises nested covariance.
#pragma warning disable IDE0300
    void Establish() => input = new ParentWithChildren(new Child[][] { new Child[] { new("nested") } });
#pragma warning restore IDE0300

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_write_the_family_identifier_and_names() => result.ShouldEqual("{\"children\":[[{\"someValue\":\"nested\",\"_derivedTypeId\":\"child\"}]],\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithChildren(IChild[][] Children) : IParent;
}
