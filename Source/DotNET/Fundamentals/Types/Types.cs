// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Collections;

namespace Cratis.Types;

/// <summary>
/// Represents an implementation of <see cref="ITypes"/>.
/// </summary>
public class Types : ITypes
{
    /// <summary>
    /// Gets the global instance of <see cref="Types"/>.
    /// </summary>
    /// <remarks>
    /// Its recommended to use the singleton defined here, rather than building your own instance.
    /// This is due to the performance impact of scanning all assemblies in the application.
    /// </remarks>
    public static readonly Types Instance = new();

    readonly ContractToImplementorsMap _contractToImplementorsMap = new();
    readonly List<Assembly> _assemblies = [];

    /// <summary>
    /// Initializes a new instance of <see cref="Types"/>.
    /// </summary>
    /// <remarks>
    /// This will automatically set up <see cref="Types"/> using generated providers when available,
    /// and otherwise use the <see cref="ProjectReferencedAssemblies"/> and <see cref="PackageReferencedAssemblies"/> providers.
    /// </remarks>
    public Types()
        : this(
            GeneratedTypeDiscoveryRegistry.Providers.Any()
                ? GeneratedTypeDiscoveryRegistry.Providers
                :
                [
                    ProjectReferencedAssemblies.Instance,
                    PackageReferencedAssemblies.Instance
                ])
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Types"/>.
    /// </summary>
    /// <param name="assemblyProviders">Collection of assembly providers.</param>
    public Types(IEnumerable<ICanProvideAssembliesForDiscovery> assemblyProviders)
    {
        var providers = assemblyProviders.ToArray();

        providers.ForEach(_ => _.Initialize());
        var assemblies = providers.SelectMany(_ => _.Assemblies).Distinct();
        _assemblies.AddRange(assemblies);
        All = providers.SelectMany(_ => _.DefinedTypes).Distinct();

        var precomputedProviders = providers.OfType<ICanProvideContractToImplementorsForDiscovery>().ToArray();
        if (precomputedProviders.Length > 0)
        {
            var contractsAndImplementors = new Dictionary<Type, HashSet<Type>>();

            foreach (var provider in precomputedProviders)
            {
                foreach (var (contract, implementors) in provider.ContractsAndImplementors)
                {
                    if (!contractsAndImplementors.TryGetValue(contract, out var currentImplementors))
                    {
                        currentImplementors = [];
                        contractsAndImplementors[contract] = currentImplementors;
                    }

                    foreach (var implementor in implementors)
                    {
                        currentImplementors.Add(implementor);
                    }
                }
            }

            _contractToImplementorsMap.Feed(contractsAndImplementors.ToDictionary(_ => _.Key, _ => _.Value.AsEnumerable()));

            // For any providers that don't have precomputed mappings (e.g. a plain assembly provider
            // passed explicitly alongside generated providers), scan their types by reflection so that
            // their implementations are still discoverable.
            var nonPrecomputedTypes = providers
                .Where(static p => p is not ICanProvideContractToImplementorsForDiscovery)
                .SelectMany(static p => p.DefinedTypes)
                .Distinct();
            _contractToImplementorsMap.Feed(nonPrecomputedTypes);

            _contractToImplementorsMap.FeedTypes(All);
            return;
        }

        _contractToImplementorsMap.Feed(All);
    }

    /// <inheritdoc/>
    public IEnumerable<Assembly> Assemblies => _assemblies;

    /// <inheritdoc/>
    public IEnumerable<Type> All { get; }

    /// <inheritdoc/>
    public Type FindSingle<T>() => FindSingle(typeof(T));

    /// <inheritdoc/>
    public IEnumerable<Type> FindMultiple<T>() => FindMultiple(typeof(T));

    /// <inheritdoc/>
    public Type FindSingle(Type type)
    {
        var typesFound = _contractToImplementorsMap.GetImplementorsFor(type);
        ThrowIfMultipleTypesFound(type, typesFound);
        return typesFound.SingleOrDefault()!;
    }

    /// <inheritdoc/>
    public IEnumerable<Type> FindMultiple(Type type)
        => _contractToImplementorsMap.GetImplementorsFor(type);

    /// <inheritdoc/>
    public Type FindTypeByFullName(string fullName)
    {
        var typeFound = _contractToImplementorsMap.All.SingleOrDefault(t => t.FullName == fullName);
        ThrowIfTypeNotFound(fullName, typeFound!);
        return typeFound!;
    }

    void ThrowIfMultipleTypesFound(Type type, IEnumerable<Type> typesFound)
    {
        if (typesFound.Count() > 1)
        {
            throw new MultipleTypesFound(type, typesFound);
        }
    }

    void ThrowIfTypeNotFound(string fullName, Type typeFound)
    {
        if (typeFound == null)
        {
            throw new UnableToResolveTypeByName(fullName);
        }
    }
}
