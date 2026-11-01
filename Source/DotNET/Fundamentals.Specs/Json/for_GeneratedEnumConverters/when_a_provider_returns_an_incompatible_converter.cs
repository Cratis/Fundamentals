// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_GeneratedEnumConverters;

public class when_a_provider_returns_an_incompatible_converter : given.enum_converter_providers
{
    void Establish() => provider.As<ICanCreateEnumConverters>()
        .Setup(factory => factory.CreateEnumConverter(typeof(DayOfWeek)))
        .Returns(new EnumConverter<ConsoleColor>());

    void Because() => result = GeneratedEnumConverters.TryCreate(typeof(DayOfWeek), [provider.Object]);

    [Fact] void should_return_null() => result.ShouldBeNull();
}
