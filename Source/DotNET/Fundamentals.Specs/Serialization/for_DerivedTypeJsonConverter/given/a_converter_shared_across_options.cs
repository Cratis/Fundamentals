// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter.given;

public class a_converter_shared_across_options : Specification
{
    protected string permissive_result;
    protected string restrictive_result;
    protected JsonSerializerOptions permissive;
    protected JsonSerializerOptions restrictive;

    void Establish()
    {
        var converter = new DerivedTypeJsonConverter<IParent>(new Mock<IDerivedTypes>().Object);
        permissive = new() { DefaultIgnoreCondition = JsonIgnoreCondition.Never };
        permissive.Converters.Add(converter);
        restrictive = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        restrictive.Converters.Add(converter);
    }

    [DerivedType("parent")]
    protected record ParentWithOptionalValue(string? Optional, int Count) : IParent;

    public interface IParent;
}
