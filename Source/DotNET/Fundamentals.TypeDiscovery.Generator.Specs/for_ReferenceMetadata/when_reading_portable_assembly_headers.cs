// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_ReferenceMetadata;

public class when_reading_portable_assembly_headers : Specification
{
    MetadataReference _reference;
    IAssemblySymbol _assembly;
    ReferenceMetadata _result;

    void Establish()
    {
        var compilation = CompatibilityInputs.Create(CompatibilityScenario.Filters);
        _reference = compilation.References.Single(reference => compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly && assembly.Name.Equals("Cratis.Filtered", StringComparison.Ordinal));
        _assembly = (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(_reference);
    }

    void Because() => _result = ReferenceMetadata.Create(_reference, TestContext.Current.CancellationToken);

    [Fact]
    void should_preserve_the_assembly_name() => _result.Name.ShouldEqual(_assembly.Name);

    [Fact]
    void should_preserve_the_dependency_names() => _result.Dependencies.ShouldEqual(_assembly.Modules.SelectMany(module => module.ReferencedAssemblies)
        .Select(identity => identity.Name).Distinct().Order(StringComparer.Ordinal));
}
