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
        var symbols = compilation.Assembly.GlobalNamespace
            .GetAllNamedTypes()
            .Where(static s => s.CanBeReferencedFromGeneratedCode())
            .ToImmutableArray();

        var namedTypes = symbols
            .Select(static s => s.GetTypeOfExpression())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToImmutableArray();

        var contractsAndImplementors = TypeDiscoveryCollector.GetContractsAndImplementors(symbols)
            .OrderBy(static e => e.ContractExpression, StringComparer.Ordinal)
            .ToImmutableArray();

        var conventionServiceBindings = TypeDiscoveryCollector.GetConventionServiceBindings(symbols)
            .OrderBy(static e => e.ServiceExpression, StringComparer.Ordinal)
            .ThenBy(static e => e.ImplementationExpression, StringComparer.Ordinal)
            .ToImmutableArray();

        var conventionSelfBindings = TypeDiscoveryCollector.GetConventionSelfBindings(symbols)
            .OrderBy(static e => e.ImplementationExpression, StringComparer.Ordinal)
            .ToImmutableArray();

        var source = GeneratedSourceBuilder.Build(namedTypes, contractsAndImplementors, conventionServiceBindings, conventionSelfBindings);
        context.AddSource("GeneratedTypeDiscoveryProvider.g.cs", source);
    }
}
