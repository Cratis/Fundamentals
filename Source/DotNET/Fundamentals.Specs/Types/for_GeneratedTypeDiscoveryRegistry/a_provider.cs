// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry;

public class a_provider : ICanProvideAssembliesForDiscovery, ICanProvideContractToImplementorsForDiscovery, ICanProvideConventionsForDependencyInjection
{
    public IEnumerable<Assembly> Assemblies => [typeof(a_provider).Assembly];

    public IEnumerable<Type> DefinedTypes => [typeof(for_ContractToImplementorsMap.IInterface), typeof(for_ContractToImplementorsMap.ImplementationOfInterface)];

    public IEnumerable<ConventionServiceBinding> ConventionServiceBindings => [];

    public IEnumerable<ConventionSelfBinding> SelfBindings =>
    [
        new(typeof(for_ContractToImplementorsMap.ImplementationOfInterface), ServiceLifetime.Transient)
    ];

    public IDictionary<Type, IEnumerable<Type>> ContractsAndImplementors => new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(for_ContractToImplementorsMap.IInterface)] = [typeof(for_ContractToImplementorsMap.ImplementationOfInterface)]
    };

    public void Initialize()
    {
    }
}