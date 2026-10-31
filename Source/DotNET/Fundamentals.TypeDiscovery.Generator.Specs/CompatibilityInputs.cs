// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs;

static class CompatibilityInputs
{
    const string Types = "namespace App; public interface IService<T> {} public abstract class Base<T> {} " +
        "public class Service : Base<int>, IService<string> {} public class Box<T> { public class Nested {} } " +
        "public class Outer { public class Nested {} private class Private {} protected class Protected {} } " +
        "public partial class Partial {} internal class Internal {} file class FileLocal {} public delegate void Callback(); " +
        "public enum Choice { None } public struct ImplicitStruct {} public struct ExplicitStruct { public ExplicitStruct() {} } " +
        "public static class Utilities {} public class Hidden { private Hidden() {} }";

    public static CSharpCompilation Create(CompatibilityScenario scenario)
    {
        var references = new[] { typeof(object).Assembly, Assembly.Load("System.Runtime"), Assembly.Load("netstandard") }
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToImmutableArray<MetadataReference>();
        var compilation = CSharpCompilation.Create(
            "Compatibility",
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return scenario switch
        {
            CompatibilityScenario.References => WithReferences(compilation),
            CompatibilityScenario.Filters => WithFilters(compilation),
            CompatibilityScenario.BuildTime => WithBuildTimeReference(compilation),
            CompatibilityScenario.Conventions => WithConventions(compilation),
            _ => WithSource(compilation, scenario)
        };
    }

    public static PortableExecutableReference Reference(CSharpCompilation compilation, string name, string source, string? path = null)
    {
        using var stream = new MemoryStream();
        var result = compilation.RemoveAllSyntaxTrees().WithAssemblyName(name).AddSyntaxTrees(Tree(source, "Reference.cs")).Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join('\n', result.Diagnostics));
        }

        return MetadataReference.CreateFromImage(stream.ToArray(), filePath: path);
    }

    static SyntaxTree Tree(string source, string path) => CSharpSyntaxTree.ParseText(source, path: path);

    static CSharpCompilation WithSource(CSharpCompilation compilation, CompatibilityScenario scenario) => scenario switch
    {
        CompatibilityScenario.Types => compilation.AddSyntaxTrees(
            Tree(Types, "Types.cs"),
            Tree("namespace App; public partial class Partial {}", "Partial.cs"),
            Tree("namespace App; public class Generated {} public partial class Partial {}", "Other.g.cs")),
        CompatibilityScenario.TopLevel => compilation.WithOptions(compilation.Options.WithOutputKind(OutputKind.ConsoleApplication))
            .AddSyntaxTrees(Tree("System.Console.WriteLine(1); public class Application {}", "Program.cs")),
        CompatibilityScenario.Empty => compilation,
        _ => throw new ArgumentOutOfRangeException(nameof(scenario))
    };

    static CSharpCompilation WithReferences(CSharpCompilation compilation)
    {
        const string packageSource = "namespace Cratis.Services; public interface IFoo {} public class Foo : IFoo {} " +
            "public interface IBox<T> {} public class Box<T> : IBox<T> {} public class Outer { public class Nested {} } " +
            "public class Generic<T> { public class Nested {} } internal class Internal {} public class Duplicate {}";
        var package = Reference(compilation, "Cratis.Services", packageSource);
        var other = Reference(compilation, "Other", "namespace Cratis.Services; public class Duplicate {} public interface IExternal {} public class External : IExternal {}");
        const string source = "namespace App { public class Local : Cratis.Services.IBox<int>, Cratis.Services.IExternal {} } " +
            "namespace Cratis.Services { public class Foo {} public class Duplicate {} }";
        return compilation.AddReferences(package, other).AddSyntaxTrees(Tree(source, "Local.cs"));
    }

    static CSharpCompilation WithBuildTimeReference(CSharpCompilation compilation)
    {
        var roslyn = Reference(compilation, "Microsoft.CodeAnalysis.Fixture", "namespace Roslyn; public class Context {}");
        compilation = compilation.AddReferences(roslyn);
        var package = Reference(compilation, "Cratis.BuildOnly", "namespace Build; public interface IService {} public class Service : IService { public Roslyn.Context Context; }");
        return compilation.AddReferences(package);
    }

    static CSharpCompilation WithConventions(CSharpCompilation compilation)
    {
        const string source = "namespace App; public interface IService {} public class Service : IService {} public abstract class Other : IService {} " +
            "public class A { public interface IMultiple {} } public class B { public interface IMultiple {} } " +
            "public class Multiple : A.IMultiple, B.IMultiple {} public interface IInherited {} public class Inherited : IInherited {} " +
            "public class Derived : Inherited {} public class Generic<T> : IInherited {} public interface IOnly {} " +
            "public class Only : IOnly {} [Cratis.DependencyInjection.IgnoreConvention] public class Ignored : IOnly {}";
        var attributes = Reference(compilation, "Cratis.Attributes", "namespace Cratis.DependencyInjection; public class IgnoreConventionAttribute : System.Attribute {}");
        return compilation.AddReferences(attributes).AddSyntaxTrees(Tree(source, "Conventions.cs"));
    }

    static CSharpCompilation WithFilters(CSharpCompilation compilation)
    {
        const string attributeSource = "namespace Cratis.DependencyInjection; public class IgnoreConventionAttribute : System.Attribute {} " +
            "public class SingletonAttribute : System.Attribute {} public class ScopedAttribute : System.Attribute {}";
        var attributes = Reference(compilation, "Cratis.Attributes", attributeSource);
        compilation = compilation.AddReferences(attributes);
        const string packageSource = "#pragma warning disable TEST001\nnamespace Services; " +
            "[System.Diagnostics.CodeAnalysis.Experimental(\"TEST001\")] public interface IExperimental {} public class Experimental : IExperimental {} " +
            "[Cratis.DependencyInjection.IgnoreConvention] public class Ignored {} public record Data(int Value); " +
            "public class Primitive(string value) {} public class RecordParameter(Data value) {} public class Failure : System.Exception {}";
        var package = Reference(compilation, "Cratis.Filtered", packageSource);
        var aliased = Reference(compilation, "Cratis.Aliased", "namespace Hidden; public interface IAlias {} public class Alias : IAlias {}")
            .WithAliases(["hidden"]);
        var tasks = Reference(compilation, "Cratis.Tasks", "namespace Build; public class TaskOnly {}", "/packages/tasks/net10.0/Cratis.Tasks.dll");
        var analyzers = Reference(compilation, "Cratis.Analyzers", "namespace Build; public class AnalyzerOnly {}", "/packages/analyzers/dotnet/Cratis.Analyzers.dll");
        const string source = "extern alias hidden; namespace App; public class Alias : hidden::Hidden.IAlias {} " +
            "public interface IFoo {} [Cratis.DependencyInjection.Singleton] public class Foo : IFoo {} " +
            "public interface IBar {} [Cratis.DependencyInjection.Scoped] public class Bar : IBar {} " +
            "[Cratis.DependencyInjection.IgnoreConvention] public class Ignored {} [System.Obsolete] public class Obsolete {} " +
            "public class Experimental : Services.IExperimental {}";
        return compilation.AddReferences(package, aliased, tasks, analyzers).AddSyntaxTrees(Tree(source, "Filters.cs"));
    }
}
