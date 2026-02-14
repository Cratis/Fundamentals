// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Concepts.for_TypesExtensions.given;

public class assembly_provider_with_defined_types(IEnumerable<Type> definedTypes) : ICanProvideAssembliesForDiscovery
{
    public IEnumerable<Assembly> Assemblies => definedTypes.Select(_ => _.Assembly).Distinct();

    public IEnumerable<Type> DefinedTypes => definedTypes;

    public void Initialize()
    {
    }
}
