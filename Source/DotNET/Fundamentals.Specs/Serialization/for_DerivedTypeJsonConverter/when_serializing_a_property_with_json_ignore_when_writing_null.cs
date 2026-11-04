// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_property_with_json_ignore_when_writing_null : given.a_set_of_derived_type_families
{
    string null_result;
    string value_result;

    void Establish() => options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

    void Because()
    {
        null_result = JsonSerializer.Serialize<IParent>(new ParentWithOptionalValue(null, 0), options);
        value_result = JsonSerializer.Serialize<IParent>(new ParentWithOptionalValue("text", 0), options);
    }

    [Fact] void should_omit_null_value() => null_result.ShouldEqual("{\"count\":0,\"_derivedTypeId\":\"parent\"}");
    [Fact] void should_write_non_null_value() => value_result.ShouldEqual("{\"optional\":\"text\",\"count\":0,\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithOptionalValue([property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Optional, int Count) : IParent;
}
