// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_generating_enum_converters.given;

public class an_enum_compilation : Specification
{
    protected string generated_source;
    protected IEnumerable<Diagnostic> errors;

    protected void Generate(string source)
    {
        var compilation = CompilationFactory.CreateCompilation(source, typeof(Enumerable).Assembly)
            .AddReferences(CreateContractsReference());
        compilation = compilation.WithOptions(compilation.Options.WithGeneralDiagnosticOption(ReportDiagnostic.Error));
        CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var withGenerated, out _);
        generated_source = withGenerated.SyntaxTrees.Except(compilation.SyntaxTrees).Single().GetText().ToString();
        errors = withGenerated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    static MetadataReference CreateContractsReference()
    {
        // Keep the generator specs independent of the runtime project while compiling the entire output.
        var contracts = CompilationFactory.CreateCompilation("""
            #nullable enable
            namespace System.Text.Json.Serialization
            {
                public abstract class JsonConverter { }
            }
            namespace Cratis.Json
            {
                public interface ICanCreateEnumConverters
                {
                    System.Text.Json.Serialization.JsonConverter? CreateEnumConverter(System.Type enumType);
                }
                public class EnumConverter<T> : System.Text.Json.Serialization.JsonConverter where T : struct, System.Enum { }
            }
            namespace Cratis.Types
            {
                public interface ICanProvideAssembliesForDiscovery { }
                public interface ICanProvideContractToImplementorsForDiscovery { }
                public static class GeneratedTypeDiscoveryRegistry
                {
                    public static void Register(ICanProvideAssembliesForDiscovery provider) { }
                }
            }
            namespace Microsoft.Extensions.DependencyInjection
            {
                public enum ServiceLifetime { Transient, Singleton, Scoped }
            }
            namespace Cratis.DependencyInjection
            {
                public interface ICanProvideConventionsForDependencyInjection { }
                public class ConventionServiceBinding
                {
                    public ConventionServiceBinding(System.Type service, System.Type implementation, Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
                }
                public class ConventionSelfBinding
                {
                    public ConventionSelfBinding(System.Type implementation, Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime) { }
                }
            }
            """).WithAssemblyName("DiscoveryContracts");
        using var stream = new MemoryStream();
        var result = contracts.Emit(stream);
        Assert.True(result.Success, string.Join("\n", result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
