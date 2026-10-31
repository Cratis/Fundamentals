// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using Cratis.Specifications.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs;

/// <summary>
/// Creates in-memory Roslyn compilations from C# source text for use in specs.
/// </summary>
static class CompilationFactory
{
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
        var inputs = Inputs();
        var syntaxTree = CSharpSyntaxTree.ParseText(source, inputs.ParseOptions);

        return CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            inputs.References(additionalAssemblies),
            inputs.Options);
    }

    static CompilationInputs Inputs()
    {
        var inputs = SpecificationResources.Get<CompilationInputs>();
        return inputs.IsCompletedSuccessfully ? inputs.Result : inputs.AsTask().GetAwaiter().GetResult();
    }
}
