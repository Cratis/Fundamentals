// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_with_default_ignore_condition_when_writing_null : given.a_set_of_derived_type_families
{
    string result;

    void Establish() => options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    void Because() => result = JsonSerializer.Serialize<IParent>(new ParentWithOptionalValue(null, 1), options);

    [Fact] void should_omit_null_value() => result.ShouldEqual("{\"count\":1,\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithOptionalValue(string? Optional, int Count) : IParent;
}
