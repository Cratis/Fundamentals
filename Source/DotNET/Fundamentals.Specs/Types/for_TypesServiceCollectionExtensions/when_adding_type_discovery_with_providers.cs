// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_TypesServiceCollectionExtensions;

public class when_adding_type_discovery_with_providers : Specification
{
    IServiceCollection _services;
    ITypes _types;
    ITypes _typesForASecondCollection;

    void Establish() => _services = new ServiceCollection();

    void Because()
    {
        _services.AddTypeDiscovery([new for_Types.plain_assembly_provider()]);
        _types = _services.BuildServiceProvider().GetRequiredService<ITypes>();

        var other = new ServiceCollection();
        other.AddTypeDiscovery([new for_Types.plain_assembly_provider()]);
        _typesForASecondCollection = other.BuildServiceProvider().GetRequiredService<ITypes>();
    }

    [Fact] void should_report_explicit_discovery() => _types.DiscoveryMode.ShouldEqual(TypeDiscoveryMode.Explicit);
    [Fact] void should_use_the_supplied_providers() => _types.Assemblies.ShouldContain(typeof(for_Types.plain_assembly_provider).Assembly);

    /// <summary>
    /// Supplying providers asks for a universe of the caller's own, so this path must keep building one
    /// per call - the reuse the default path gained must not quietly extend to it.
    /// </summary>
    [Fact] void should_build_its_own_universe_per_call() => _typesForASecondCollection.ShouldNotBeSame(_types);
}
