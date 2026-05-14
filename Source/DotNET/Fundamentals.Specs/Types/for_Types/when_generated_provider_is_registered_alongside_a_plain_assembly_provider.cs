// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_Types;

public class when_generated_provider_is_registered_alongside_a_plain_assembly_provider : Specification
{
    Type[] _implementors;

    void Because() => _implementors =
    [
        ..new Types([new for_GeneratedTypeDiscoveryRegistry.a_provider(), new plain_assembly_provider()])
            .FindMultiple<IPackageOnlyInterface>()
    ];

    [Fact] void should_discover_types_from_the_plain_assembly_provider() => _implementors.ShouldContainOnly(typeof(PackageOnlyImplementation));
}

public interface IPackageOnlyInterface;

public class PackageOnlyImplementation : IPackageOnlyInterface;

class plain_assembly_provider : ICanProvideAssembliesForDiscovery
{
    public IEnumerable<Assembly> Assemblies => [typeof(plain_assembly_provider).Assembly];

    public IEnumerable<Type> DefinedTypes => [typeof(IPackageOnlyInterface), typeof(PackageOnlyImplementation)];

    public void Initialize()
    {
    }
}
