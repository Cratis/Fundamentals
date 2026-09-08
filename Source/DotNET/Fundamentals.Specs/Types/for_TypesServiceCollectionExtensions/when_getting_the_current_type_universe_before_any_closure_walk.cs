// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_TypesServiceCollectionExtensions;

public class when_getting_the_current_type_universe_before_any_closure_walk : Specification
{
    const string _probeAssemblyName = "Cratis.Fundamentals.Specs.ModuleInitializerProbe";
    const string _probeTypeFullName = "Cratis.Fundamentals.Specs.ModuleInitializerProbe.ModuleInitializerProbeService";

    Assembly _probeAssembly;
    ITypes _universe;

    /// <summary>
    /// Loads the probe without registering anything: <see cref="Assembly.Load(string)"/> does not run a
    /// module constructor, so nothing but a closure walk registers the probe's generated provider.
    /// </summary>
    void Establish() => _probeAssembly = Assembly.Load(_probeAssemblyName);

    /// <summary>
    /// Whether this call is the first walk in the process depends on which spec class ran before it; the
    /// contract holds either way. Run alone, this class is red without the walk inside the accessor.
    /// </summary>
    void Because() => _universe = TypesServiceCollectionExtensions.CurrentTypeUniverse();

    [Fact] void should_register_the_probe_assembly_provider() => GeneratedTypeDiscoveryRegistry.Providers.Count(_ => _.GetType().Assembly == _probeAssembly).ShouldNotEqual(0);
    [Fact] void should_cover_the_probe_assembly_types() => _universe.All.Select(_ => _.FullName).ShouldContain(_probeTypeFullName);
}
