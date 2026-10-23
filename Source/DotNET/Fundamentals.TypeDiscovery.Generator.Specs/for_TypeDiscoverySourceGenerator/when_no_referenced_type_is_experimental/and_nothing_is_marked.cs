// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_no_referenced_type_is_experimental;

/// <summary>
/// Without any <c language="csharp">[Experimental]</c> types the suppressed diagnostics stay exactly the obsolete ones.
/// </summary>
public class and_nothing_is_marked : Specification
{
    string _generatedSource;

    void Establish()
    {
        var compilation = CompilationFactory.CreateCompilation("namespace App { public class MyApp { } }");
        var result = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator()).RunGenerators(compilation).GetRunResult();
        _generatedSource = result.GeneratedTrees.Single().GetText().ToString();
    }

    [Fact] void should_only_suppress_the_obsolete_diagnostics() => _generatedSource.ShouldContain("#pragma warning disable CS0612, CS0618\n");
}
