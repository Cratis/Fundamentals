// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Source generator that emits compile-time type discovery metadata for the current assembly.
/// </summary>
[Generator]
public sealed class TypeDiscoverySourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider,
            static (sourceProductionContext, compilation) => Execute(sourceProductionContext, compilation));
    }

    static void Execute(SourceProductionContext context, Compilation compilation)
    {
        // The synthesized Program class for top-level statements is the entry-point container. It
        // cannot be meaningfully referenced from generated code, and its unqualified global-namespace
        // name conflicts with Program classes in referenced assemblies, producing CS0436.
        var entryPointContainerType = compilation.GetEntryPoint(CancellationToken.None)?.ContainingType;

        var symbols = compilation.Assembly.GlobalNamespace
            .GetAllNamedTypes()
            .Where(s => s.CanBeReferencedFromGeneratedCode() &&
                        !SymbolEqualityComparer.Default.Equals(s, entryPointContainerType))
            .ToImmutableArray();

        var namedTypes = symbols
            .Select(static s => s.GetTypeOfExpression())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToImmutableArray();

        var contractsAndImplementors = TypeDiscoveryCollector.GetContractsAndImplementors(symbols)
            .OrderBy(static e => e.ContractExpression, StringComparer.Ordinal)
            .ToImmutableArray();

        // Collect convention bindings from public types in referenced Cratis.* assemblies.
        // These packages do not ship their own generated provider, so their I<X>/X pairs
        // would otherwise be invisible once the generated path is taken (bypassing the
        // reflection fallback that previously discovered them).
        var packageSymbols = compilation.References
            .Select(r => compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol)
            .Where(static a => a?.Name.StartsWith("Cratis", StringComparison.Ordinal) is true)
            .Cast<IAssemblySymbol>()
            .SelectMany(static a => a.GlobalNamespace.GetAllNamedTypes())
            .Where(static s =>
                s.DeclaredAccessibility == Accessibility.Public &&
                !s.IsImplicitlyDeclared &&
                !s.IsAnonymousType &&
                !s.IsFileLocal &&
                s.CanBeReferencedByName)
            .ToImmutableArray();

        var conventionServiceBindings = TypeDiscoveryCollector.GetConventionServiceBindings(symbols)
            .Concat(TypeDiscoveryCollector.GetConventionServiceBindings(packageSymbols))
            .OrderBy(static e => e.ServiceExpression, StringComparer.Ordinal)
            .ThenBy(static e => e.ImplementationExpression, StringComparer.Ordinal)
            .ToImmutableArray();

        var conventionSelfBindings = TypeDiscoveryCollector.GetConventionSelfBindings(symbols)
            .Concat(TypeDiscoveryCollector.GetConventionSelfBindings(packageSymbols))
            .OrderBy(static e => e.ImplementationExpression, StringComparer.Ordinal)
            .ToImmutableArray();

        var source = GeneratedSourceBuilder.Build(namedTypes, contractsAndImplementors, conventionServiceBindings, conventionSelfBindings);
        context.AddSource("GeneratedTypeDiscoveryProvider.g.cs", source);
    }
}
