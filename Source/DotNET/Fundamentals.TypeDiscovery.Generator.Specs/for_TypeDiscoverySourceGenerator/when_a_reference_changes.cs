// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator;

public class when_a_reference_changes : Specification
{
    CSharpCompilation _compilation;
    MetadataReference _reference;
    GeneratorDriver _driver;
    GeneratorDriverRunResult _result;

    void Establish()
    {
        _compilation = CompatibilityInputs.Create(CompatibilityScenario.Empty);
        _reference = CompatibilityInputs.Reference(_compilation, "Cratis.Changing", "namespace Services; public class Before {}", "/lib/Cratis.Changing.dll");
        _compilation = _compilation.AddReferences(_reference);
        _driver = CSharpGeneratorDriver.Create(
            generators: [new TypeDiscoverySourceGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        _driver = _driver.RunGenerators(_compilation);
    }

    void Because()
    {
        var replacement = CompatibilityInputs.Reference(_compilation.RemoveReferences(_reference), "Cratis.Changing", "namespace Services; public class After {}", "/lib/Cratis.Changing.dll");
        _result = _driver.RunGenerators(_compilation.ReplaceReference(_reference, replacement)).GetRunResult();
    }

    [Fact]
    void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();

    [Fact]
    void should_discover_the_replacement() => _result.GeneratedTrees.Single().GetText(TestContext.Current.CancellationToken).ToString().ShouldContain("typeof(global::Services.After)");

    [Fact]
    void should_not_retain_the_previous_types() => _result.GeneratedTrees.Single().GetText(TestContext.Current.CancellationToken).ToString().ShouldNotContain("typeof(global::Services.Before)");

    [Fact]
    void should_invalidate_the_changed_module() => _result.Results.Single().TrackedSteps["ReferenceDiscovery"]
        .SelectMany(step => step.Outputs).ShouldContain(output => output.Reason == IncrementalStepRunReason.Modified &&
            ((ReferenceDiscovery)output.Value).Identity.StartsWith("Cratis.Changing,", StringComparison.Ordinal));
}
