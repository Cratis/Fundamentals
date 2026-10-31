// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        var references = context.MetadataReferencesProvider.WithComparer(new MetadataReferenceComparer());
        var referenceTypes = references.Combine(references.Collect())
            .Select(static (input, cancellationToken) => ReferenceDiscovery.Create(input.Left, input.Right, cancellationToken))
            .WithTrackingName("ReferenceDiscovery");
        var currentTypes = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax,
                static (syntaxContext, cancellationToken) => DiscoverCurrentType(syntaxContext, cancellationToken))
            .Where(static type => type is not null)
            .WithTrackingName("CurrentAssemblyDiscovery");
        var source = currentTypes.Collect().Combine(referenceTypes.Collect())
            .Select(static (input, _) => DiscoveryOutput.Build(input.Left.Select(static type => type!), input.Right))
            .WithTrackingName("TypeDiscoverySource");
        context.RegisterSourceOutput(source, static (productionContext, text) => productionContext.AddSource("GeneratedTypeDiscoveryProvider.g.cs", text));
    }

    static DiscoveredType? DiscoverCurrentType(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (!symbol.CanBeReferencedFromGeneratedCode() || symbol.IsFromSourceGenerator())
        {
            return null;
        }

        var entryPoint = context.SemanticModel.Compilation.GetEntryPoint(cancellationToken)?.ContainingType;
        if (SymbolEqualityComparer.Default.Equals(symbol, entryPoint))
        {
            return null;
        }

        // Partial declarations contribute exactly once, including when another part is generated.
        var declaration = symbol.DeclaringSyntaxReferences[0];
        if (declaration.SyntaxTree != context.Node.SyntaxTree || declaration.Span != context.Node.Span)
        {
            return null;
        }

        return new DiscoveredType(symbol, context.SemanticModel.Compilation.Assembly);
    }
}
