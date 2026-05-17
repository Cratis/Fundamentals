// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_assembly_is_referenced_with_non_global_alias;

/// <summary>
/// Verifies that types from assemblies referenced with a non-global C# alias are excluded from
/// the generated type discovery output, preventing CS0234/CS0246 build errors.
/// </summary>
public class and_the_aliased_assembly_contains_a_public_type : Specification
{
    const string AliasName = "RuntimeAlias";

    GeneratorDriverRunResult _result;
    string _generatedSource;

    void Establish()
    {
        // System.Runtime has many public types (e.g. System.Int32). Reference it with a non-global
        // alias to simulate the scenario described in the issue.
        var compilation = CompilationFactory.CreateCompilationWithAliasedReference(
            "namespace App { public class MyClass { } }",
            Assembly.Load("System.Runtime"),
            AliasName);

        var generator = new TypeDiscoverySourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        _result = driver.RunGenerators(compilation).GetRunResult();
        _generatedSource = _result.GeneratedTrees.SingleOrDefault()?.GetText().ToString() ?? string.Empty;
    }

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_produce_generated_source() => _generatedSource.ShouldNotBeEmpty();
    [Fact] void should_include_the_local_type() => _generatedSource.ShouldContain("typeof(global::App.MyClass)");
    [Fact] void should_not_reference_types_from_aliased_assembly() =>
        _generatedSource.ShouldNotContain("typeof(global::System.Int32)");
}
