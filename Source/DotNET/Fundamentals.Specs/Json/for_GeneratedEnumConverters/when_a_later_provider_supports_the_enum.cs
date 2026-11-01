// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_GeneratedEnumConverters;

public class when_a_later_provider_supports_the_enum : given.enum_converter_providers
{
    void Establish()
    {
        provider.As<ICanCreateEnumConverters>()
            .Setup(factory => factory.CreateEnumConverter(typeof(DayOfWeek)))
            .Returns(new EnumConverter<ConsoleColor>());
        later_provider.As<ICanCreateEnumConverters>()
            .Setup(factory => factory.CreateEnumConverter(typeof(DayOfWeek)))
            .Returns(converter);
    }

    void Because() => result = GeneratedEnumConverters.TryCreate(typeof(DayOfWeek), [non_factory.Object, provider.Object, later_provider.Object]);

    [Fact] void should_return_the_compatible_converter() => result.ShouldEqual(converter);
}
