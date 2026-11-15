// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.Loader;
using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_a_referenced_package_type_is_removed.given;

/// <summary>
/// Builds <c language="csharp">Cratis.Pkg</c> in two versions with the same identity - the old one with <c language="csharp">Gone</c> and <c language="csharp">Stays</c>,
/// the newer one without <c language="csharp">Gone</c> - and an application compiled, with the generator, against the old one.
/// The application is then loaded against the newer package, as happens when a consumer upgrades a package
/// without rebuilding an assembly that was built against the previous major.
/// </summary>
public class an_application_built_against_an_older_package : Specification
{
    const string OlderPackage = "namespace Cratis.Pkg { public class Gone { } public class Stays { } public interface IGonePair { } public class GonePair : IGonePair { } public interface IStaysPair { } public class StaysPair : IStaysPair { } }";
    const string NewerPackage = "namespace Cratis.Pkg { public class Stays { } public interface IStaysPair { } public class StaysPair : IStaysPair { } }";

    /// <summary>The generator specs do not reference Fundamentals, so the few types the generated provider implements are stubbed.</summary>
    const string Stubs =
        "namespace Microsoft.Extensions.DependencyInjection { public enum ServiceLifetime { Singleton, Scoped, Transient } }\n" +
        "namespace Cratis.DependencyInjection {\n" +
        "  public record ConventionSelfBinding(System.Type ImplementationType, Microsoft.Extensions.DependencyInjection.ServiceLifetime Lifetime);\n" +
        "  public record ConventionServiceBinding(System.Type ServiceType, System.Type ImplementationType, Microsoft.Extensions.DependencyInjection.ServiceLifetime Lifetime);\n" +
        "  public interface ICanProvideConventionsForDependencyInjection {\n" +
        "    System.Collections.Generic.IEnumerable<ConventionServiceBinding> ConventionServiceBindings { get; }\n" +
        "    System.Collections.Generic.IEnumerable<ConventionSelfBinding> SelfBindings { get; } } }\n" +
        "namespace Cratis.Types {\n" +
        "  public interface ICanProvideAssembliesForDiscovery {\n" +
        "    System.Collections.Generic.IEnumerable<System.Reflection.Assembly> Assemblies { get; }\n" +
        "    System.Collections.Generic.IEnumerable<System.Type> DefinedTypes { get; }\n" +
        "    void Initialize(); }\n" +
        "  public interface ICanProvideContractToImplementorsForDiscovery {\n" +
        "    System.Collections.Generic.IDictionary<System.Type, System.Collections.Generic.IEnumerable<System.Type>> ContractsAndImplementors { get; } }\n" +
        "  public static class GeneratedTypeDiscoveryRegistry {\n" +
        "    public static object Provider;\n" +
        "    public static void Register(ICanProvideAssembliesForDiscovery provider) => Provider = provider; } }";

    protected IEnumerable<Type> _selfBindings;
    protected IEnumerable<(Type Service, Type Implementation)> _serviceBindings;
    protected string _generatedSource;
    protected Exception _exception;

    string _directory;
    AssemblyLoadContext _context;

    void Establish()
    {
        _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);

        var olderPath = Path.Combine(_directory, "Cratis.Pkg.dll");
        File.WriteAllBytes(olderPath, Emit(CreateCompilation(OlderPackage).WithAssemblyName("Cratis.Pkg")));

        var application = CreateCompilation("namespace App { public class MyApp { } public interface IOwnPair { } public class OwnPair : IOwnPair { } } namespace Zeta { public class Late { } public interface ILatePair { } public class LatePair : ILatePair { } }");
        application = application.AddReferences(MetadataReference.CreateFromFile(olderPath));
        CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator()).RunGeneratorsAndUpdateCompilation(application, out var withGenerated, out _);
        _generatedSource = withGenerated.SyntaxTrees.Last().GetText().ToString();
        var applicationBytes = Emit(withGenerated.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Stubs)));

        var newerBytes = Emit(CreateCompilation(NewerPackage).WithAssemblyName("Cratis.Pkg"));
        _context = new NewerPackageContext(newerBytes);
        var assembly = _context.LoadFromStream(new MemoryStream(applicationBytes));
        var providerType = assembly.GetTypes().Single(type => type.Name.EndsWith("GeneratedTypeDiscoveryProvider", StringComparison.Ordinal));
        _provider = Activator.CreateInstance(providerType, nonPublic: true);
    }

    object _provider;

    /// <summary>Reads both binding collections of the generated provider, capturing any exception.</summary>
    protected void ReadBindings()
    {
        try
        {
            var providerType = _provider.GetType();
            _selfBindings = [.. ((System.Collections.IEnumerable)providerType.GetProperty("SelfBindings").GetValue(_provider)).Cast<object>().Select(binding => (Type)binding.GetType().GetProperty("ImplementationType").GetValue(binding))];
            _serviceBindings = [.. ((System.Collections.IEnumerable)providerType.GetProperty("ConventionServiceBindings").GetValue(_provider)).Cast<object>()
                .Select(binding => ((Type)binding.GetType().GetProperty("ServiceType")!.GetValue(binding)!, (Type)binding.GetType().GetProperty("ImplementationType")!.GetValue(binding)!))];
        }
        catch (Exception exception)
        {
            _exception = exception is TargetInvocationException { InnerException: { } inner } ? inner : exception;
        }
    }

    void Destroy()
    {
        _context?.Unload();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    static CSharpCompilation CreateCompilation(string source) =>
        CompilationFactory.CreateCompilation(source, typeof(Enumerable).Assembly, typeof(System.Collections.Generic.List<>).Assembly);

    static byte[] Emit(Compilation compilation)
    {
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
        }

        return stream.ToArray();
    }

    sealed class NewerPackageContext(byte[] newerPackage) : AssemblyLoadContext("NewerPackage", isCollectible: true)
    {
        protected override Assembly Load(AssemblyName assemblyName) =>
            string.Equals(assemblyName.Name, "Cratis.Pkg", StringComparison.Ordinal) ? LoadFromStream(new MemoryStream(newerPackage)) : null!;
    }
}
