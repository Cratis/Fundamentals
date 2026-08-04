// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_TypesServiceCollectionExtensions;

public class when_adding_type_discovery_with_providers : Specification
{
    ITypes _types;

    void Because()
    {
        var services = new ServiceCollection();
        services.AddTypeDiscovery([new for_Types.plain_assembly_provider()]);
        _types = services.BuildServiceProvider().GetRequiredService<ITypes>();
    }

    [Fact] void should_report_explicit_discovery() => _types.DiscoveryMode.ShouldEqual(TypeDiscoveryMode.Explicit);
    [Fact] void should_use_the_supplied_providers() => _types.Assemblies.ShouldContain(typeof(for_Types.plain_assembly_provider).Assembly);
}
