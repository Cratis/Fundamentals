// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter.given;

public class a_set_of_derived_type_families : Specification
{
    protected JsonSerializerOptions options;

    void Establish()
    {
        var derivedTypes = new Mock<IDerivedTypes>();
        derivedTypes.Setup(_ => _.HasDerivatives(typeof(IParent))).Returns(true);
        derivedTypes.Setup(_ => _.HasDerivatives(typeof(IChild))).Returns(true);
        derivedTypes.Setup(_ => _.HasDerivatives(typeof(INode))).Returns(true);
        derivedTypes.Setup(_ => _.GetDerivedTypeFor(typeof(IParent), "parent")).Returns(typeof(Parent));
        derivedTypes.Setup(_ => _.GetDerivedTypeFor(typeof(IChild), "child")).Returns(typeof(Child));
        derivedTypes.Setup(_ => _.GetDerivedTypeFor(typeof(INode), "node")).Returns(typeof(Node));
        options = new()
        {
            PropertyNamingPolicy = new DefaultNamingPolicy().JsonPropertyNamingPolicy,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new DerivedTypeJsonConverterFactory(derivedTypes.Object));
    }

    public interface IParent;
    public interface IChild;
    public interface INode;

    [DerivedType("parent")]
    public record Parent(IChild? Child) : IParent;

    [DerivedType("child")]
    public record Child(string SomeValue) : IChild;

    [DerivedType("node")]
    public record Node(string Name, INode? Child) : INode;
}
