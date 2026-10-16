// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_types_are_nested_in_open_generic_types;

/// <summary>
/// Verifies that types nested in open generic types, declared in a referenced Cratis assembly, are skipped
/// just like they are in the current assembly, since <c language="csharp">typeof(Key&lt;T&gt;.Part)</c> cannot be emitted.
/// </summary>
public class and_the_types_are_in_a_referenced_cratis_assembly : Specification
{
    const string ReferencedSource = "namespace Cratis.Nested { public class Key<T> { public class Part { } public class GenericPart<U> { } public class Middle { public class Deep { } } }  public class Plain { } }";

    string _tempDir;
    GeneratorDriverRunResult _result;
    string _generatedSource;

    void Establish()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "lib", "net10.0");
        Directory.CreateDirectory(_tempDir);
    }

    void Because()
    {
        var dllPath = Path.Combine(_tempDir, "Cratis.Nested.dll");
        var emit = CSharpCompilation.Create(
            "Cratis.Nested",
            [CSharpSyntaxTree.ParseText(ReferencedSource)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .Emit(dllPath);
        if (!emit.Success)
        {
            throw new InvalidOperationException("Failed to emit Cratis.Nested assembly.");
        }

        var baseCompilation = CompilationFactory.CreateCompilation("namespace App { public class MyApp { } public class Local<T> { public class Part { } public class GenericPart<U> { } public class Middle { public class Deep { } } } }");
        var compilation = CSharpCompilation.Create(
            baseCompilation.AssemblyName,
            baseCompilation.SyntaxTrees,
            [.. baseCompilation.References, MetadataReference.CreateFromFile(dllPath)],
            baseCompilation.Options);

        _result = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator())
            .RunGenerators(compilation)
            .GetRunResult();
        _generatedSource = _result.GeneratedTrees.SingleOrDefault()?.GetText().ToString() ?? string.Empty;
    }

    void Destroy() => Directory.Delete(Path.GetFullPath(Path.Combine(_tempDir, "..", "..")), recursive: true);

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_include_plain_referenced_type() => _generatedSource.ShouldContain("global::Cratis.Nested.Plain");
    [Fact] void should_include_the_current_assembly_type() => _generatedSource.ShouldContain("typeof(global::App.MyApp)");
    [Fact] void should_not_emit_type_nested_in_generic() => _generatedSource.ShouldNotContain(".Part");
    [Fact] void should_not_emit_nested_generic_in_generic() => _generatedSource.ShouldNotContain("GenericPart");
    [Fact] void should_not_emit_deeper_nested_type() => _generatedSource.ShouldNotContain("Deep");
    [Fact] void should_not_emit_local_type_nested_in_generic() => _generatedSource.ShouldNotContain("Local<T>.");
}
