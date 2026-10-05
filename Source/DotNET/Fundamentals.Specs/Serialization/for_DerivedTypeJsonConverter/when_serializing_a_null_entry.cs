// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_null_entry : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new Parent(null);

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_write_null_despite_the_ignore_condition() => result.ShouldEqual("{\"child\":null,\"_derivedTypeId\":\"parent\"}");
}
