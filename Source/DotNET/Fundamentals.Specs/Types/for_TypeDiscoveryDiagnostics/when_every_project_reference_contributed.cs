// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Cratis.Types.for_TypeDiscoveryDiagnostics;

public class when_every_project_reference_contributed : Specification
{
    ITypes _types;
    string[] _missing;

    void Establish() => _types = new Types([new every_project_reference()]);

    void Because() => _missing = [.. TypeDiscoveryDiagnostics.FindMissingContributors(_types)];

    [Fact] void should_report_nothing_missing() => _missing.ShouldBeEmpty();

    /// <summary>
    /// Contributes exactly the assemblies the dependency model calls project references, which is the
    /// set the diagnostic compares against.
    /// </summary>
    class every_project_reference : ICanProvideAssembliesForDiscovery
    {
        public IEnumerable<Assembly> Assemblies =>
            DependencyContext.Load(Assembly.GetEntryAssembly()!)!.RuntimeLibraries
                .Where(_ => _.Type.Equals("project", StringComparison.Ordinal))
                .Select(_ => AssemblyHelpers.Resolve(_.Name)!)
                .Where(_ => _ is not null);

        public IEnumerable<Type> DefinedTypes => [];

        public void Initialize()
        {
        }
    }
}
