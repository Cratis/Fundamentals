// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_collecting_package_symbols.given;

/// <summary>
/// Builds an in-memory Cratis.BuildOnly assembly with a public service interface/implementation pair
/// that the generator would normally emit convention bindings for. Each derived spec materializes
/// the assembly under a different NuGet folder convention to verify that the path-based filter
/// excludes build-time-only references.
/// </summary>
public class a_compilation_referencing_a_cratis_assembly : Specification
{
    protected GeneratorDriverRunResult _result;
    protected string _generatedSource;
    string _tempDir;
    byte[] _assemblyBytes;

    void Establish()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        using var stream = new MemoryStream();
        var emitResult = CSharpCompilation.Create(
            "Cratis.BuildOnly",
            [CSharpSyntaxTree.ParseText("namespace Cratis.BuildOnly { public interface IBuildOnlyService { } public class BuildOnlyService : IBuildOnlyService { } }")],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .Emit(stream);

        if (!emitResult.Success)
        {
            throw new InvalidOperationException("Failed to emit in-memory Cratis.BuildOnly assembly.");
        }

        _assemblyBytes = stream.ToArray();
    }

    void Destroy()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    /// <summary>
    /// Writes the in-memory assembly to <paramref name="relativeFolder"/> under a fresh temp directory,
    /// builds a compilation that references it from that path, runs the generator, and captures the result.
    /// </summary>
    /// <param name="relativeFolder">The relative folder (e.g. <c>tasks/net10.0</c>) under which to materialize the assembly.</param>
    protected void RunGeneratorWithAssemblyAt(string relativeFolder)
    {
        var directory = Path.Combine(_tempDir, relativeFolder);
        Directory.CreateDirectory(directory);
        var dllPath = Path.Combine(directory, "Cratis.BuildOnly.dll");
        File.WriteAllBytes(dllPath, _assemblyBytes);

        var assemblyReference = MetadataReference.CreateFromFile(dllPath);
        var baseCompilation = CompilationFactory.CreateCompilation("namespace App { public class MyApp { } }");
        var compilation = CSharpCompilation.Create(
            baseCompilation.AssemblyName,
            baseCompilation.SyntaxTrees,
            [.. baseCompilation.References, assemblyReference],
            baseCompilation.Options);

        _result = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator())
            .RunGenerators(compilation)
            .GetRunResult();
        _generatedSource = _result.GeneratedTrees.SingleOrDefault()?.GetText().ToString() ?? string.Empty;
    }
}
