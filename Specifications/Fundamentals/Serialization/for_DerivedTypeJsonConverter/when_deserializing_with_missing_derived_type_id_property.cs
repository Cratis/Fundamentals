// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Serialization.for_DerivedTypeJsonConverterFactory;

public class when_deserializing_with_missing_derived_type_id_property : Specification
{
    interface ITargetType;
    Mock<IDerivedTypes> derived_types;
    DerivedTypeJsonConverter<ITargetType> converter;
    ITargetType result;

    void Establish()
    {
        derived_types = new();
        converter = new(derived_types.Object);
    }

    void Because()
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("{}").AsSpan());
        result = converter.Read(ref reader, typeof(ITargetType), new JsonSerializerOptions());
    }

    [Fact] void should_return_null_value() => result.ShouldBeNull();
}
