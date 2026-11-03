// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_non_default_nullable_value_that_equals_the_underlying_default : given.a_set_of_derived_type_families
{
    string zero_result;
    string null_result;

    void Because()
    {
        zero_result = JsonSerializer.Serialize<IParent>(new ParentWithNullableCount(0), options);
        null_result = JsonSerializer.Serialize<IParent>(new ParentWithNullableCount(null), options);
    }

    [Fact] void should_write_the_zero() => zero_result.ShouldEqual("{\"count\":0,\"_derivedTypeId\":\"parent\"}");
    [Fact] void should_omit_the_null() => null_result.ShouldEqual("{\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithNullableCount([property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] int? Count) : IParent;
}
