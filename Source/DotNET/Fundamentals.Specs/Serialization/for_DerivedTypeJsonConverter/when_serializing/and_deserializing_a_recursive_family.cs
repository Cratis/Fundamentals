// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter.when_serializing;

public class and_deserializing_a_recursive_family : given.a_set_of_derived_type_families
{
    string input;
    INode result;

    void Establish() => input = JsonSerializer.Serialize<INode>(new Node("root", new Node("branch", new Node("leaf", null))), options);

    void Because() => result = JsonSerializer.Deserialize<INode>(input, options)!;

    [Fact] void should_preserve_the_leaf_on_round_trip() => ((Node)((Node)((Node)result).Child!).Child!).Name.ShouldEqual("leaf");
}
