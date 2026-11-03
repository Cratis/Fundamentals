// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_derived_type_with_inherited_properties : given.a_set_of_derived_type_families
{
    string result;

    void Because() => result = JsonSerializer.Serialize<IParent>(new DerivedWithInheritance("base-value", "own-value"), options);

    [Fact] void should_write_the_inherited_property() => result.ShouldContain("\"baseValue\":\"base-value\"");
    [Fact] void should_write_the_own_property() => result.ShouldContain("\"ownValue\":\"own-value\"");

    class Base
    {
        public string BaseValue { get; set; } = string.Empty;
    }

    [DerivedType("parent")]
    class DerivedWithInheritance : Base, IParent
    {
        public DerivedWithInheritance(string baseValue, string ownValue)
        {
            BaseValue = baseValue;
            OwnValue = ownValue;
        }

        public string OwnValue { get; set; }
    }
}
