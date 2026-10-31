// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_preserving_generated_output;

public class when_type_arguments_have_the_same_name_in_different_assemblies : Specification
{
    byte[] _source;
    byte[] _package;

    void Because()
    {
        var empty = CompatibilityInputs.Create(CompatibilityScenario.Empty);
        const string dependency = "namespace Shared; public class Value {} public interface IRepo<T> {}";
        var left = CompatibilityInputs.Reference(empty, "Left", dependency).WithAliases(["left"]);
        var right = CompatibilityInputs.Reference(empty, "Right", dependency).WithAliases(["right"]);
        var compilation = empty.AddReferences(left, right);
        const string source = "extern alias left; extern alias right; namespace App; public class Container<T> { public interface IRepo {} } " +
            "public class Repo : Container<left::Shared.Value>.IRepo {} public class CachingRepo : Container<right::Shared.Value>.IRepo {}";
        _source = Generate(compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, path: "Types.cs")));
        var reference = CompatibilityInputs.Reference(compilation, "Cratis.Shapes", source);
        _package = Generate(compilation.AddReferences(reference));
    }

    [Fact]
    void should_preserve_the_original_source_bindings() => _source.ShouldEqual(Expected(false));

    [Fact]
    void should_preserve_the_original_package_bindings() => _package.ShouldEqual(Expected(true));

    static byte[] Generate(CSharpCompilation compilation)
    {
        var result = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator()).RunGenerators(compilation).GetRunResult();
        result.Diagnostics.ShouldBeEmpty();
        return Encoding.UTF8.GetBytes(result.GeneratedTrees.Single().GetText().ToString());
    }

    static byte[] Expected(bool package)
    {
        var suffix = package ? ".aliased-arguments-package.txt" : ".aliased-arguments-source.txt";
        var assembly = typeof(when_type_arguments_have_the_same_name_in_different_assemblies).Assembly;
        using var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(name => name.EndsWith(suffix, StringComparison.Ordinal)));
        using var bytes = new MemoryStream();
        stream.CopyTo(bytes);
        return bytes.ToArray();
    }
}
