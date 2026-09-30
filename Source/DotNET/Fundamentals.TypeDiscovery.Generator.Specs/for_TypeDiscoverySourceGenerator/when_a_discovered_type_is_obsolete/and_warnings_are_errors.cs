// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_a_discovered_type_is_obsolete;

/// <summary>
/// A type marked <c language="csharp">[Obsolete]</c> is still discovered, and listing it must not fail a project that treats
/// warnings as errors - the generated source is not the project's to edit.
/// </summary>
public class and_warnings_are_errors : Specification
{
    string _generatedSource;
    IEnumerable<Diagnostic> _errors;

    void Establish()
    {
        var compilation = CompilationFactory.CreateCompilation(
            "namespace App;\n" +
            "[System.Obsolete(\"Use something else\")]\n" +
            "public class Retired { }");

        compilation = compilation.WithOptions(compilation.Options.WithGeneralDiagnosticOption(ReportDiagnostic.Error));

        CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var withGenerated, out _);

        var generatedTree = withGenerated.SyntaxTrees.Except(compilation.SyntaxTrees).Single();
        _generatedSource = generatedTree.GetText().ToString();
        _errors = withGenerated.GetDiagnostics().Where(diagnostic => string.Equals(diagnostic.Id, "CS0612", StringComparison.Ordinal) || string.Equals(diagnostic.Id, "CS0618", StringComparison.Ordinal));
    }

    [Fact] void should_discover_the_obsolete_type() => _generatedSource.ShouldContain("typeof(global::App.Retired)");
    [Fact] void should_not_report_it_as_obsolete() => _errors.ShouldBeEmpty();
}
