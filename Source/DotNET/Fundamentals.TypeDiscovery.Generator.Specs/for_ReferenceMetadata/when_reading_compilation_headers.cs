// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_ReferenceMetadata;

public class when_reading_compilation_headers : Specification
{
    Compilation _compilation;
    ReferenceMetadata _result;

    void Establish() => _compilation = CompatibilityInputs.Create(CompatibilityScenario.Conventions);

    void Because() => _result = ReferenceMetadata.Create(_compilation.ToMetadataReference(), TestContext.Current.CancellationToken);

    [Fact]
    void should_preserve_the_assembly_name() => _result.Name.ShouldEqual(_compilation.Assembly.Name);

    [Fact]
    void should_preserve_the_dependency_names() => _result.Dependencies.ShouldEqual(_compilation.Assembly.Modules.SelectMany(module => module.ReferencedAssemblies)
        .Select(identity => identity.Name).Distinct().Order(StringComparer.Ordinal));
}
