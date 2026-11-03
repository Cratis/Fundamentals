// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_positional_record_deriving_from_a_base_record : given.a_set_of_derived_type_families
{
    string result;

    void Because() => result = JsonSerializer.Serialize<IParent>(new Dog("Rex", 3), options);

    [Fact] void should_write_the_base_record_property() => result.ShouldContain("\"name\":\"Rex\"");
    [Fact] void should_write_the_own_property() => result.ShouldContain("\"age\":3");

    abstract record Animal(string Name);

    [DerivedType("parent")]
    record Dog(string Name, int Age) : Animal(Name), IParent;
}
