// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_Types;

public class when_a_provider_yields_a_type_that_cannot_be_loaded : Specification
{
    Exception _error;
    Type[] _implementors;

    void Because() => _error = Catch.Exception(() =>
        _implementors =
        [
            ..new Types([new provider_that_throws_while_enumerating(), new plain_assembly_provider()])
                .FindMultiple<IPackageOnlyInterface>()
        ]);

    [Fact] void should_not_fail() => _error.ShouldBeNull();
    [Fact] void should_still_discover_types_from_the_loadable_provider() => _implementors.ShouldContainOnly(typeof(PackageOnlyImplementation));

    /// <summary>
    /// A provider whose type enumeration throws <see cref="TypeLoadException"/> partway through, simulating a
    /// referenced assembly that resolves to a version no longer containing a referenced type.
    /// </summary>
    class provider_that_throws_while_enumerating : ICanProvideAssembliesForDiscovery
    {
        public IEnumerable<Assembly> Assemblies => [typeof(provider_that_throws_while_enumerating).Assembly];

        public IEnumerable<Type> DefinedTypes
        {
            get
            {
                yield return typeof(object);
                throw new TypeLoadException("Simulated missing referenced type");
            }
        }

        public void Initialize()
        {
        }
    }
}
