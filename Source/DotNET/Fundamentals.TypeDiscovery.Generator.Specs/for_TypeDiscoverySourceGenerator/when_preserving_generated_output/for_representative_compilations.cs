// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_preserving_generated_output;

public class for_representative_compilations
{
    [Theory]
    [InlineData((int)CompatibilityScenario.Types)]
    [InlineData((int)CompatibilityScenario.References)]
    [InlineData((int)CompatibilityScenario.Filters)]
    [InlineData((int)CompatibilityScenario.TopLevel)]
    [InlineData((int)CompatibilityScenario.Empty)]
    [InlineData((int)CompatibilityScenario.BuildTime)]
    [InlineData((int)CompatibilityScenario.Conventions)]
    public void should_match_the_original_generator_byte_for_byte(int scenarioId)
    {
        var scenario = (CompatibilityScenario)scenarioId;
        var name = scenario switch
        {
            CompatibilityScenario.BuildTime => "build-time",
            CompatibilityScenario.TopLevel => "top-level",
            _ => scenario.ToString().ToLowerInvariant()
        };
        var assembly = typeof(for_representative_compilations).Assembly;
        using var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(resource => resource.EndsWith($".{name}.txt", StringComparison.Ordinal)));
        using var expected = new MemoryStream();
        stream.CopyTo(expected);
        var result = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator()).RunGenerators(CompatibilityInputs.Create(scenario), TestContext.Current.CancellationToken).GetRunResult();
        result.Diagnostics.ShouldBeEmpty();
        Encoding.UTF8.GetBytes(result.GeneratedTrees.Single().GetText(TestContext.Current.CancellationToken).ToString()).ShouldEqual(expected.ToArray());
    }
}
