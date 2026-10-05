// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_with_default_naming_policy : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish() => input = new Parent(new Child("nested"));

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_use_immediate_camel_case_names_for_each_family() => result.ShouldEqual("{\"child\":{\"someValue\":\"nested\",\"_derivedTypeId\":\"child\"},\"_derivedTypeId\":\"parent\"}");
}
