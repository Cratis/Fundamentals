// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_with_dictionary_key_policy : given.a_set_of_derived_type_families
{
    IParent input;
    string result;

    void Establish()
    {
        options.DictionaryKeyPolicy = new PrefixNamingPolicy();
        input = new Parent(new Child("nested"));
    }

    void Because() => result = JsonSerializer.Serialize(input, options);

    [Fact] void should_apply_the_policy_to_camel_case_names_and_identifiers() => result.ShouldEqual("{\"key_child\":{\"key_someValue\":\"nested\",\"key__derivedTypeId\":\"child\"},\"key__derivedTypeId\":\"parent\"}");

    sealed class PrefixNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => $"key_{name}";
    }
}
