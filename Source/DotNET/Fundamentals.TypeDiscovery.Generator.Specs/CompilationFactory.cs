// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs;

/// <summary>
/// Creates in-memory Roslyn compilations from C# source text for use in specs.
/// </summary>
static class CompilationFactory
{
    static readonly Assembly[] _defaultAssemblies =
    [
        typeof(object).Assembly,
        Assembly.Load("System.Runtime"),
        Assembly.Load("netstandard"),
    ];

    /// <summary>
    /// Compiles the given C# source text and returns all named type symbols from the resulting assembly.
    /// </summary>
    /// <param name="source">The C# source text to compile.</param>
    /// <param name="additionalAssemblies">Optional additional reference assemblies beyond the defaults.</param>
    /// <returns>All named type symbols from the compilation's global namespace.</returns>
    public static ImmutableArray<INamedTypeSymbol> GetNamedTypes(string source, params Assembly[] additionalAssemblies)
    {
        var compilation = CreateCompilation(source, additionalAssemblies);
        return [.. compilation.Assembly.GlobalNamespace.GetAllNamedTypes()];
    }

    /// <summary>
    /// Creates a Roslyn <see cref="CSharpCompilation"/> for the given source text.
    /// </summary>
    /// <param name="source">The C# source text to compile.</param>
    /// <param name="additionalAssemblies">Optional additional reference assemblies beyond the defaults.</param>
    /// <returns>A <see cref="CSharpCompilation"/> for the given source.</returns>
    public static CSharpCompilation CreateCompilation(string source, params Assembly[] additionalAssemblies)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        SyntaxTree[] syntaxTrees = [syntaxTree];
        MetadataReference[] references =
        [
            .. _defaultAssemblies
                .Concat(additionalAssemblies)
                .Distinct()
                .Select(static asm => (MetadataReference)MetadataReference.CreateFromFile(asm.Location))
        ];

        return CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    /// <summary>
    /// Creates a Roslyn <see cref="CSharpCompilation"/> for the given source text where one of the
    /// additional assemblies is referenced with a non-global C# alias (simulating
    /// <c>&lt;Aliases&gt;SomeAlias&lt;/Aliases&gt;</c> in a project file).
    /// </summary>
    /// <param name="source">The C# source text to compile.</param>
    /// <param name="aliasedAssembly">The assembly to reference with a non-global alias.</param>
    /// <param name="alias">The alias to apply to <paramref name="aliasedAssembly"/>.</param>
    /// <returns>A <see cref="CSharpCompilation"/> with the aliased reference.</returns>
    public static CSharpCompilation CreateCompilationWithAliasedReference(string source, Assembly aliasedAssembly, string alias)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var aliasedReference = MetadataReference.CreateFromFile(
            aliasedAssembly.Location,
            new MetadataReferenceProperties(aliases: [alias]));
        MetadataReference[] references =
        [
            .. _defaultAssemblies
                .Select(static asm => (MetadataReference)MetadataReference.CreateFromFile(asm.Location)),
            aliasedReference
        ];

        return CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }
}
