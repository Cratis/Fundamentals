// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_a_referenced_type_is_experimental;

/// <summary>
/// A referenced type marked <c language="csharp">[Experimental]</c> - directly, through a containing type or through its
/// assembly - is still listed, and doing so must not fail a project that treats warnings as errors.
/// </summary>
public class and_warnings_are_errors : Specification
{
    string _generatedSource;
    IEnumerable<Diagnostic> _errors;

    void Establish()
    {
        using var stream = new MemoryStream();
        var emitResult = CSharpCompilation.Create(
            "Cratis.Packages",
            [CSharpSyntaxTree.ParseText(
                "[assembly: System.Diagnostics.CodeAnalysis.Experimental(\"ASM001\")]\n" +
                "namespace Cratis.Packages\n" +
                "{\n" +
                "    [System.Diagnostics.CodeAnalysis.Experimental(\"TYPE001\")]\n" +
                "    public class Outer { public class Inner { } }\n" +
                "    [System.Diagnostics.CodeAnalysis.Experimental(\"TYPE001\")]\n" +
                "    public class Duplicate { }\n" +
                "}")],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .Emit(stream);

        if (!emitResult.Success)
        {
            throw new InvalidOperationException("Failed to emit in-memory Cratis.Packages assembly.");
        }

        stream.Position = 0;
        var baseCompilation = CompilationFactory.CreateCompilation("namespace App { public class MyApp { } }");
        var compilation = CSharpCompilation.Create(
            baseCompilation.AssemblyName,
            baseCompilation.SyntaxTrees,
            [.. baseCompilation.References, MetadataReference.CreateFromStream(stream)],
            baseCompilation.Options.WithGeneralDiagnosticOption(ReportDiagnostic.Error));

        CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var withGenerated, out _);

        var generatedTree = withGenerated.SyntaxTrees.Except(compilation.SyntaxTrees).Single();
        _generatedSource = generatedTree.GetText().ToString();
        _errors = withGenerated.GetDiagnostics().Where(diagnostic => string.Equals(diagnostic.Id, "ASM001", StringComparison.Ordinal) || string.Equals(diagnostic.Id, "TYPE001", StringComparison.Ordinal));
    }

    [Fact] void should_discover_the_experimental_type() => _generatedSource.ShouldContain("typeof(global::Cratis.Packages.Outer)");
    [Fact] void should_discover_the_nested_type() => _generatedSource.ShouldContain("typeof(global::Cratis.Packages.Outer.Inner)");
    [Fact] void should_suppress_each_id_once_in_sorted_order() => _generatedSource.ShouldContain("#pragma warning disable CS0612, CS0618, ASM001, TYPE001\n");
    [Fact] void should_not_report_the_experimental_diagnostics() => _errors.ShouldBeEmpty();
}
