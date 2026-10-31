// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator;

public class when_a_reference_dependency_changes : Specification
{
    CSharpCompilation _empty;
    CSharpCompilation _compilation;
    MetadataReference _dependency;
    GeneratorDriver _driver;
    GeneratorDriverRunResult _result;
    string _fresh;

    void Establish()
    {
        _empty = CompatibilityInputs.Create(CompatibilityScenario.Empty);
        _dependency = CompatibilityInputs.Reference(_empty, "Dependency", "namespace Contracts; public class Base {}", "/lib/Dependency.dll");
        var consumer = CompatibilityInputs.Reference(_empty.AddReferences(_dependency), "Cratis.Consumer", "namespace Services; public class Service : Contracts.Base {}");
        _compilation = _empty.AddReferences(_dependency, consumer);
        _driver = CSharpGeneratorDriver.Create(
            generators: [new TypeDiscoverySourceGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        _driver = _driver.RunGenerators(_compilation);
        _driver.GetRunResult().GeneratedTrees.Single().GetText().ToString().ShouldContain("typeof(global::Services.Service)");
    }

    void Because()
    {
        var replacement = CompatibilityInputs.Reference(_empty, "Dependency", "namespace Contracts; public class Base : System.Exception {}", "/lib/Dependency.dll");
        var compilation = _compilation.ReplaceReference(_dependency, replacement);
        _result = _driver.RunGenerators(compilation).GetRunResult();
        _fresh = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator()).RunGenerators(compilation).GetRunResult().GeneratedTrees.Single().GetText().ToString();
    }

    [Fact]
    void should_invalidate_the_dependent_package() => _result.Results.Single().TrackedSteps["ReferenceDiscovery"]
        .SelectMany(step => step.Outputs).ShouldContain(output => output.Reason == IncrementalStepRunReason.Modified &&
            ((ReferenceDiscovery)output.Value).Name == "Cratis.Consumer");

    [Fact]
    void should_apply_the_updated_exception_filter() => _result.GeneratedTrees.Single().GetText(TestContext.Current.CancellationToken).ToString().ShouldNotContain("typeof(global::Services.Service)");

    [Fact]
    void should_match_a_fresh_run() => _result.GeneratedTrees.Single().GetText(TestContext.Current.CancellationToken).ToString().ShouldEqual(_fresh);
}
