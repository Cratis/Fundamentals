// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Cratis.Json;

namespace Cratis.Json.for_GeneratedEnumConverters.given;

public class enum_converter_providers : Specification
{
    protected Mock<ICanProvideAssembliesForDiscovery> provider;
    protected Mock<ICanProvideAssembliesForDiscovery> later_provider;
    protected Mock<ICanProvideAssembliesForDiscovery> non_factory;
    protected JsonConverter converter;
    protected JsonConverter result;

    void Establish()
    {
        provider = new Mock<ICanProvideAssembliesForDiscovery>();
        provider.As<ICanCreateEnumConverters>();
        later_provider = new Mock<ICanProvideAssembliesForDiscovery>();
        later_provider.As<ICanCreateEnumConverters>();
        non_factory = new Mock<ICanProvideAssembliesForDiscovery>();
        converter = new EnumConverter<DayOfWeek>();
    }
}
