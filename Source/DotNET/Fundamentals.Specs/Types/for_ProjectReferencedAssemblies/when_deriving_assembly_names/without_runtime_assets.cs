// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyModel;

namespace Cratis.Types.for_ProjectReferencedAssemblies.when_deriving_assembly_names;

public class without_runtime_assets : Specification
{
    RuntimeLibrary _library;
    IEnumerable<string> _names;

    void Establish() => _library = new RuntimeLibrary(
        type: "project",
        name: "App.Contracts",
        version: "1.0.0",
        hash: string.Empty,
        runtimeAssemblyGroups: [],
        nativeLibraryGroups: [],
        resourceAssemblies: [],
        dependencies: [],
        serviceable: false);

    void Because() => _names = ProjectReferencedAssemblies.GetAssemblyNames([_library]);

    [Fact] void should_fall_back_to_the_library_name() => _names.ShouldContainOnly("App.Contracts");
}
