// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_ReferenceMetadata;

public class when_reading_module_headers : Specification
{
    MetadataReference _reference;
    ReferenceMetadata _result;

    void Establish()
    {
        var compilation = CSharpCompilation.Create(
            "Module",
            references: CompatibilityInputs.Create(CompatibilityScenario.Conventions).References,
            options: new CSharpCompilationOptions(OutputKind.NetModule));
        using var image = new MemoryStream();
        compilation.Emit(image).Success.ShouldBeTrue();
        _reference = MetadataReference.CreateFromImage(image.ToArray(), new MetadataReferenceProperties(MetadataImageKind.Module));
    }

    void Because() => _result = ReferenceMetadata.Create(_reference, TestContext.Current.CancellationToken);

    [Fact]
    void should_have_no_assembly_name() => _result.Name.ShouldEqual(string.Empty);

    [Fact]
    void should_have_no_assembly_dependencies() => _result.Dependencies.ShouldBeEmpty();
}
