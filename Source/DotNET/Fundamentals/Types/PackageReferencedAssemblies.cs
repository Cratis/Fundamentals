// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Aksio.Types;

/// <summary>
/// Represents an implementation of <see cref="ICanProvideAssembliesForDiscovery"/> that provides package referenced assemblies.
/// </summary>
public class PackageReferencedAssemblies : ICanProvideAssembliesForDiscovery
{
    /// <summary>
    /// Gets the instance of <see cref="PackageReferencedAssemblies"/>.
    /// </summary>
    /// <remarks>
    /// Its recommended to use the singleton defined here, rather than building your own instance.
    /// This is due to the performance impact of scanning all assemblies in the application.
    /// </remarks>
    public static readonly PackageReferencedAssemblies Instance = new();

    readonly List<string> _assemblyPrefixesToExclude = new()
    {
        "System",
        "Microsoft",
        "Newtonsoft",
        "runtimepack",
        "mscorlib",
        "netstandard",
        "WindowsBase",
        "Namotion",
        "Semver",
        "NJsonSchema",
        "Humanizer"
    };

    readonly List<string> _assemblyPrefixesToInclude = new()
    {
        "Aksio"
    };

    readonly List<Assembly> _assemblies = new();

    /// <inheritdoc/>
    public IEnumerable<Assembly> Assemblies => _assemblies;

    /// <inheritdoc/>
    public IEnumerable<Type> DefinedTypes { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Types"/>.
    /// </summary>
    public PackageReferencedAssemblies()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var dependencyModel = DependencyContext.Load(entryAssembly);

        var assemblies = dependencyModel.RuntimeLibraries
                            .Where(_ => _.RuntimeAssemblyGroups.Count > 0 &&
                                        (_assemblyPrefixesToInclude.Any(asm => _.Name.StartsWith(asm)) ||
                                        !_assemblyPrefixesToExclude.Any(asm => _.Name.StartsWith(asm))))
                            .Select(_ => AssemblyHelpers.Resolve(_.Name)!)
                            .Where(_ => _ is not null)
                            .Distinct()
                            .ToArray();
        _assemblies.AddRange(assemblies);
        DefinedTypes = _assemblies.SelectMany(_ => _.DefinedTypes);
    }

    /// <summary>
    /// Add an assembly prefix to exclude from type discovery.
    /// </summary>
    /// <param name="prefixes">Prefixes to add.</param>
    public void AddAssemblyPrefixesToExclude(params string[] prefixes) => _assemblyPrefixesToExclude.AddRange(prefixes);
}