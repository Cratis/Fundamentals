// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_recursive_family : given.a_set_of_derived_type_families
{
    INode input;
    string result;

    void Establish() => input = new Node("root", new Node("branch", new Node("leaf", null)));

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_write_the_branch_type_identifier() => JsonDocument.Parse(result).RootElement.GetProperty("child").GetProperty("_derivedTypeId").GetString().ShouldEqual("node");
    [Fact] void should_write_the_leaf_type_identifier() => JsonDocument.Parse(result).RootElement.GetProperty("child").GetProperty("child").GetProperty("_derivedTypeId").GetString().ShouldEqual("node");
}
