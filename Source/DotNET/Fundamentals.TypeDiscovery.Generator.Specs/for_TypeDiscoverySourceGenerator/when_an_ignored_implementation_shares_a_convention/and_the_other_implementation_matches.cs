// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_an_ignored_implementation_shares_a_convention;

public class and_the_other_implementation_matches : Specification
{
    CSharpCompilation _compilation;
    GeneratorDriverRunResult _result;
    string _generatedSource;

    void Establish() =>
        _compilation = CompilationFactory.CreateCompilation(
            "namespace Cratis.DependencyInjection { public class IgnoreConventionAttribute : System.Attribute { } }\n" +
            "namespace App {\n" +
            "public interface IFoo { }\n" +
            "public class Foo : IFoo { }\n" +
            "[Cratis.DependencyInjection.IgnoreConvention] public class AnotherFoo : IFoo { }\n" +
            "}");

    void Because()
    {
        _result = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator()).RunGenerators(_compilation).GetRunResult();
        _generatedSource = _result.GeneratedTrees.Single().GetText().ToString();
    }

    [Fact] void should_compile_the_input_without_errors() => _compilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    [Fact] void should_not_report_generator_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_emit_the_convention_service_binding() => _generatedSource.ShouldContain("new global::Cratis.DependencyInjection.ConventionServiceBinding(typeof(global::App.IFoo), typeof(global::App.Foo), global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)");
}
