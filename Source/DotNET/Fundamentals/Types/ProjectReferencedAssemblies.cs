// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Cratis.Types;

/// <summary>
/// Represents an implementation of <see cref="ICanProvideAssembliesForDiscovery"/> that provides project referenced assemblies.
/// </summary>
public class ProjectReferencedAssemblies : ICanProvideAssembliesForDiscovery
{
    /// <summary>
    /// Gets the global instance of <see cref="PackageReferencedAssemblies"/>.
    /// </summary>
    /// <remarks>
    /// Its recommended to use the singleton defined here, rather than building your own instance.
    /// This is due to the performance impact of scanning all assemblies in the application.
    /// </remarks>
    public static readonly ProjectReferencedAssemblies Instance = new();
#pragma warning disable MA0158, IDE0330 // Cannot use Lock in .net 8
    static readonly object _lock = new();
#pragma warning restore

    readonly List<Assembly> _assemblies = [];
    bool _initialized;

    /// <inheritdoc/>
    public IEnumerable<Assembly> Assemblies => _assemblies;

    /// <inheritdoc/>
    public IEnumerable<Type> DefinedTypes { get; private set; } = [];

    /// <inheritdoc/>
    public void Initialize()
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return;
            }

            var entryAssembly = Assembly.GetEntryAssembly()!;
            var dependencyModel = DependencyContext.Load(entryAssembly);
            if (dependencyModel is null) return;

            var projectReferencedAssemblies = dependencyModel.RuntimeLibraries
                                .Where(_ => _.Type.Equals("project"))
                                .Select(_ => AssemblyHelpers.Resolve(_.Name)!)
                                .Where(_ => _ is not null)
                                .Distinct()
                                .ToArray();
            _assemblies.AddRange(projectReferencedAssemblies);
            DefinedTypes = [.. _assemblies.SelectMany(_ => _.DefinedTypes)];

            _initialized = true;
        }
    }
}
