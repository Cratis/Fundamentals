// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_assembly_is_referenced_with_non_global_alias;

/// <summary>
/// Verifies that when a Cratis.* assembly is referenced with a non-global alias, the source generator
/// does not emit convention bindings or type references for types in that assembly, which would
/// otherwise produce CS0234/CS0246 build errors in the generated file.
/// </summary>
public class and_the_aliased_assembly_is_a_cratis_package : Specification
{
    GeneratorDriverRunResult _result;
    string _generatedSource;

    void Establish()
    {
        // Build an in-memory "Cratis.Packages" assembly with a public I<X>/X convention pair.
        // When referenced globally the generator would normally produce convention bindings for them.
        // When referenced with a non-global alias, those types must be excluded.
        using var pkgStream = new MemoryStream();
        CSharpCompilation.Create(
            "Cratis.Packages",
            [CSharpSyntaxTree.ParseText("namespace Cratis.Packages { public interface IMyService { } public class MyService : IMyService { } }")],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .Emit(pkgStream);
        pkgStream.Position = 0;

        var aliasedPkgRef = MetadataReference.CreateFromStream(
            pkgStream, new MetadataReferenceProperties(aliases: ["PkgAlias"]));

        var baseCompilation = CompilationFactory.CreateCompilation("namespace App { public class MyApp { } }");
        var compilationWithAlias = CSharpCompilation.Create(
            baseCompilation.AssemblyName,
            baseCompilation.SyntaxTrees,
            [.. baseCompilation.References, aliasedPkgRef],
            baseCompilation.Options);

        var generator = new TypeDiscoverySourceGenerator();
        _result = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilationWithAlias)
            .GetRunResult();
        _generatedSource = _result.GeneratedTrees.SingleOrDefault()?.GetText().ToString() ?? string.Empty;
    }

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_produce_generated_source() => _generatedSource.ShouldNotBeEmpty();
    [Fact] void should_include_the_current_assembly_type() => _generatedSource.ShouldContain("typeof(global::App.MyApp)");
    [Fact] void should_not_include_service_interface_from_aliased_assembly() =>
        _generatedSource.ShouldNotContain("Cratis.Packages.IMyService");
    [Fact] void should_not_include_implementation_type_from_aliased_assembly() =>
        _generatedSource.ShouldNotContain("Cratis.Packages.MyService");
}
