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

        // Only consider references that are accessible via the global:: prefix.
        // When a project reference is declared with a non-global C# alias (e.g. <Aliases>MyAlias</Aliases>),
        // its types are hidden from the global namespace and cannot be referenced via global::. Generating
        // typeof() expressions for them would produce CS0234/CS0246 build errors.
        var globallyAccessibleReferences = compilation.References
            .Where(static r => r.Properties.Aliases.IsDefaultOrEmpty || r.Properties.Aliases.Contains("global"))
            .ToArray();

        // Group all public types from every referenced assembly by their fully-qualified name so we can
        // detect two categories of conflict before emitting typeof() expressions:
        //   CS0436 — a type in the current assembly shares a FQDN with a type in a referenced assembly.
        //   CS0433 — a FQDN appears in more than one referenced assembly (ambiguous reference).
        var referencedTypesByFqdn = globallyAccessibleReferences
            .Select(r => compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol)
            .Where(static a => a is not null)
            .Cast<IAssemblySymbol>()
            .SelectMany(static a => a.GlobalNamespace.GetAllNamedTypes()
                .Where(static s => s.DeclaredAccessibility == Accessibility.Public)
                .Select(s => (Fqdn: s.GetTypeOfExpression(), AssemblyName: a.Name)))
            .ToLookup(static x => x.Fqdn, StringComparer.Ordinal);

        // FQDNs present in any referenced assembly — used to suppress CS0436 for current-assembly types.
        var referencedFqdns = new HashSet<string>(
            referencedTypesByFqdn.Select(static g => g.Key),
            StringComparer.Ordinal);

        // FQDNs present in more than one referenced assembly — used to suppress CS0433 for contracts
        // and convention bindings that the compiler cannot resolve unambiguously.
        var ambiguousFqdns = new HashSet<string>(
            referencedTypesByFqdn
                .Where(static g => g.Select(static x => x.AssemblyName).Distinct(StringComparer.Ordinal).Count() > 1)
                .Select(static g => g.Key),
            StringComparer.Ordinal);

        var symbols = compilation.Assembly.GlobalNamespace
            .GetAllNamedTypes()
            .Where(s => s.CanBeReferencedFromGeneratedCode() &&
                        !s.IsFromSourceGenerator() &&
                        !referencedFqdns.Contains(s.GetTypeOfExpression()) &&
                        !SymbolEqualityComparer.Default.Equals(s, entryPointContainerType))
            .ToImmutableArray();

        var namedTypes = symbols
            .Select(static s => s.GetTypeOfExpression())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToImmutableArray();

        var contractsAndImplementors = TypeDiscoveryCollector.GetContractsAndImplementors(symbols, compilation.Assembly, ambiguousFqdns)
            .OrderBy(static e => e.ContractExpression, StringComparer.Ordinal)
            .ToImmutableArray();

        // Collect convention bindings from public types in referenced Cratis.* assemblies.
        // These packages do not ship their own generated provider, so their I<X>/X pairs
        // would otherwise be invisible once the generated path is taken (bypassing the
        // reflection fallback that previously discovered them).
        // Assemblies that themselves reference Microsoft.CodeAnalysis are Roslyn analyzer
        // packages (e.g. Cratis.Metrics.Roslyn). Their types cannot be instantiated at
        // runtime without CodeAnalysis.dll in the output, so they are excluded.
        var packageSymbols = globallyAccessibleReferences
            .Select(r => compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol)
            .Where(static a => a?.Name.StartsWith("Cratis", StringComparison.Ordinal) is true)
            .Cast<IAssemblySymbol>()
            .Where(static a => !a.Modules.Any(static m => m.ReferencedAssemblies.Any(
                static r => r.Name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal))))
            .SelectMany(static a => a.GlobalNamespace.GetAllNamedTypes())
            .Where(s =>
                s.DeclaredAccessibility == Accessibility.Public &&
                !s.IsImplicitlyDeclared &&
                !s.IsAnonymousType &&
                !s.IsFileLocal &&
                s.CanBeReferencedByName &&
                !ambiguousFqdns.Contains(s.GetTypeOfExpression()))
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
