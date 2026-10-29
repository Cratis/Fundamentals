// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyModel;

namespace Cratis.Types.for_ProjectReferencedAssemblies.when_deriving_assembly_names;

public class with_a_non_project_library : Specification
{
    RuntimeLibrary _library;
    IEnumerable<string> _names;

    void Establish() => _library = new RuntimeLibrary(
        type: "package",
        name: "Cratis.App.Contracts",
        version: "1.0.0",
        hash: string.Empty,
        runtimeAssemblyGroups: [new RuntimeAssetGroup(string.Empty, ["App.Contracts.dll"])],
        nativeLibraryGroups: [],
        resourceAssemblies: [],
        dependencies: [],
        serviceable: false);

    void Because() => _names = ProjectReferencedAssemblies.GetAssemblyNames([_library]);

    [Fact] void should_exclude_the_library() => _names.ShouldBeEmpty();
}
