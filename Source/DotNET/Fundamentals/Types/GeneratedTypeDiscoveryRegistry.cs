// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types;

/// <summary>
/// Holds compile-time generated type discovery providers registered by consuming assemblies.
/// </summary>
public static class GeneratedTypeDiscoveryRegistry
{
#if NET9_0_OR_GREATER
    static readonly Lock _gate = new();
#else
    static readonly object _gate = new();
#endif
    static readonly List<ICanProvideAssembliesForDiscovery> _providers = [];

    /// <summary>
    /// Gets the currently registered providers.
    /// </summary>
    public static IEnumerable<ICanProvideAssembliesForDiscovery> Providers
    {
        get
        {
            lock (_gate)
            {
                return [.. _providers];
            }
        }
    }

    /// <summary>
    /// Registers a generated provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    public static void Register(ICanProvideAssembliesForDiscovery provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_gate)
        {
            if (_providers.Exists(_ => _.GetType() == provider.GetType()))
            {
                return;
            }

            _providers.Add(provider);
        }
    }
}