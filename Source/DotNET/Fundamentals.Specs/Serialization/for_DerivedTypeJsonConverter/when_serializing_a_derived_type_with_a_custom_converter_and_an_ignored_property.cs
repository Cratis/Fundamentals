// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_a_derived_type_with_a_custom_converter_and_an_ignored_property : given.a_set_of_derived_type_families
{
    string result;

    void Because() => result = JsonSerializer.Serialize<IParent>(new Secretive("visible", "secret-text"), options);

    [Fact] void should_write_the_visible_property() => result.ShouldContain("\"name\":\"visible\"");
    [Fact] void should_not_write_the_ignored_property() => result.ShouldNotContain("secret-text");

    class NoOpConverter : JsonConverter<Secretive>
    {
        public override Secretive? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null;

        public override void Write(Utf8JsonWriter writer, Secretive value, JsonSerializerOptions options) => writer.WriteNullValue();
    }

    [DerivedType("parent")]
    [JsonConverter(typeof(NoOpConverter))]
    record Secretive(string Name, [property: JsonIgnore] string Secret) : IParent;
}
