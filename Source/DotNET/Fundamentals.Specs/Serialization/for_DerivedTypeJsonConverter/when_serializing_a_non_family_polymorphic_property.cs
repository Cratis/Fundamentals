// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_non_family_polymorphic_property : given.derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new ParentWithDetail(new ExtendedDetail("detail", "runtime field"));

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_keep_the_runtime_type_fields() => JsonDocument.Parse(result).RootElement.GetProperty("detail").GetProperty("ExtraValue").GetString().ShouldEqual("runtime field");

    [DerivedType("parent")]
    record ParentWithDetail(Detail Detail) : IParent;
    record Detail(string Name);
    record ExtendedDetail(string Name, string ExtraValue) : Detail(Name);
}
