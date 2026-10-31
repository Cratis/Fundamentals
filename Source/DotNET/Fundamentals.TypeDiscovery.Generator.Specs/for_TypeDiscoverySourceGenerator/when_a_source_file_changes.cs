// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator;

public class when_a_source_file_changes : Specification
{
    CSharpCompilation _compilation;
    GeneratorDriver _driver;
    GeneratorDriverRunResult _result;

    void Establish()
    {
        _compilation = CompatibilityInputs.Create(CompatibilityScenario.References);
        _driver = CSharpGeneratorDriver.Create(
            generators: [new TypeDiscoverySourceGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        _driver = _driver.RunGenerators(_compilation);
    }

    void Because()
    {
        var tree = _compilation.SyntaxTrees.Single();
        var edited = CSharpSyntaxTree.ParseText(tree.GetText() + "\nnamespace App { public class Added {} }", path: tree.FilePath);
        _result = _driver.RunGenerators(_compilation.ReplaceSyntaxTree(tree, edited)).GetRunResult();
    }

    [Fact]
    void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();

    [Fact]
    void should_have_reference_steps() => _result.Results.Single().TrackedSteps["ReferenceDiscovery"].ShouldNotBeEmpty();

    [Fact]
    void should_reuse_every_reference_result() => _result.Results.Single().TrackedSteps["ReferenceDiscovery"]
        .SelectMany(step => step.Outputs).ShouldEachConformTo(output => output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged);

    [Fact]
    void should_update_the_generated_types() => _result.GeneratedTrees.Single().GetText(TestContext.Current.CancellationToken).ToString().ShouldContain("typeof(global::App.Added)");
}
