// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_a_referenced_package_type_is_removed;

/// <summary>An assembly that only has its own types keeps the plain generated output, without any guarded entries.</summary>
public class and_no_package_types_are_referenced : Specification
{
    string _generatedSource;

    void Because()
    {
        var compilation = CompilationFactory.CreateCompilation("namespace App { public class MyApp { } }");
        _generatedSource = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator()).RunGenerators(compilation).GetRunResult().GeneratedTrees.Single().GetText().ToString();
    }

    [Fact] void should_bind_the_own_type() => _generatedSource.ShouldContain("new global::Cratis.DependencyInjection.ConventionSelfBinding(typeof(global::App.MyApp)");
    [Fact] void should_not_guard_any_binding() => _generatedSource.ShouldNotContain("TryAdd");
}
