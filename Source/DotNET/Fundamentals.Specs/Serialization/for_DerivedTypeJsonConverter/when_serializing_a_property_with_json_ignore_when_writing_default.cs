// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_property_with_json_ignore_when_writing_default : given.a_set_of_derived_type_families
{
    string default_result;
    string value_result;

    void Establish() => options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

    void Because()
    {
        default_result = JsonSerializer.Serialize<IParent>(new ParentWithOptionalCount(0), options);
        value_result = JsonSerializer.Serialize<IParent>(new ParentWithOptionalCount(3), options);
    }

    [Fact] void should_omit_default_value() => default_result.ShouldEqual("{\"_derivedTypeId\":\"parent\"}");
    [Fact] void should_write_non_default_value() => value_result.ShouldEqual("{\"count\":3,\"_derivedTypeId\":\"parent\"}");

    [DerivedType("parent")]
    record ParentWithOptionalCount([property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] int Count) : IParent;
}
