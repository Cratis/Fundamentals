// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_local_type_implements_interface_from_non_global_aliased_assembly;

/// <summary>
/// Verifies that a local implementation of an interface from a non-global aliased assembly does not
/// cause the generator to emit contract or DI convention references that are unreachable via <c>global::</c>.
/// </summary>
public class and_interface_matches_convention : Specification
{
    GeneratorDriverRunResult _result;
    string _generatedSource;

    void Establish()
    {
        using var aliasedAssemblyStream = new MemoryStream();
        var emitResult = CSharpCompilation.Create(
            "Cratis.Packages",
            [CSharpSyntaxTree.ParseText("namespace App { public interface IFoo { } }")],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .Emit(aliasedAssemblyStream);

        if (!emitResult.Success)
        {
            throw new InvalidOperationException("Failed to emit in-memory Cratis.Packages assembly.");
        }

        aliasedAssemblyStream.Position = 0;

        var aliasedReference = MetadataReference.CreateFromStream(
            aliasedAssemblyStream,
            new MetadataReferenceProperties(aliases: ["PkgAlias"]));

        var baseCompilation = CompilationFactory.CreateCompilation(
            "extern alias PkgAlias;\n" +
            "using PkgAlias::App;\n" +
            "namespace App;\n" +
            "public class Foo : IFoo { }");

        var compilationWithAlias = CSharpCompilation.Create(
            baseCompilation.AssemblyName,
            baseCompilation.SyntaxTrees,
            [.. baseCompilation.References, aliasedReference],
            baseCompilation.Options);

        var generator = new TypeDiscoverySourceGenerator();
        _result = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilationWithAlias)
            .GetRunResult();
        _generatedSource = _result.GeneratedTrees.SingleOrDefault()?.GetText().ToString() ?? string.Empty;
    }

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_produce_generated_source() => _generatedSource.ShouldNotBeEmpty();
    [Fact] void should_include_the_local_implementation_type() => _generatedSource.ShouldContain("typeof(global::App.Foo)");
    [Fact] void should_not_include_the_aliased_contract_type() => _generatedSource.ShouldNotContain("typeof(global::App.IFoo)");
}
