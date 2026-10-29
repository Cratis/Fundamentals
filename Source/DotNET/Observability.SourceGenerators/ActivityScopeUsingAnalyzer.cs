// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Metrics.Roslyn;

/// <summary>
/// Enforces disposing trace activity scopes with a using declaration or statement.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ActivityScopeUsingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for activity scope disposal enforcement.
    /// </summary>
    public const string DiagnosticId = "CRT0001";

    static readonly DiagnosticDescriptor _rule = new(
        DiagnosticId,
        "IActivityScope<T> must be used with using",
        "IActivityScope<T> must be assigned with using var to ensure the span is stopped and parent-child relationships are maintained",
        "Traces",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
    }

    static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var type = context.SemanticModel.GetTypeInfo(invocation, context.CancellationToken).ConvertedType;
        if (!IsActivityScope(type))
        {
            return;
        }

        if (IsWithinUsing(invocation))
        {
            return;
        }

        var expression = SkipTransparentNodes(invocation);
        if (expression.Parent is ExpressionStatementSyntax ||
            expression.Parent is AssignmentExpressionSyntax ||
            expression.Parent is EqualsValueClauseSyntax)
        {
            context.ReportDiagnostic(Diagnostic.Create(_rule, invocation.GetLocation()));
        }
    }

    static bool IsActivityScope(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        if (namedTypeSymbol.OriginalDefinition.ToDisplayString() == "Cratis.Traces.IActivityScope<T>")
        {
            return true;
        }

        return namedTypeSymbol.AllInterfaces.Any(_ => _.OriginalDefinition.ToDisplayString() == "Cratis.Traces.IActivityScope<T>");
    }

    static bool IsWithinUsing(SyntaxNode node)
    {
        var current = SkipTransparentNodes(node);

        if (current.Parent is UsingStatementSyntax usingStatement &&
            (usingStatement.Expression == current || usingStatement.Declaration is not null))
        {
            return true;
        }

        if (current.Parent is EqualsValueClauseSyntax equalsValue &&
            equalsValue.Parent is VariableDeclaratorSyntax &&
            equalsValue.Parent.Parent?.Parent is LocalDeclarationStatementSyntax localDeclaration)
        {
            return localDeclaration.UsingKeyword != default;
        }

        return current.Parent is EqualsValueClauseSyntax usingEqualsValue &&
               usingEqualsValue.Parent is VariableDeclaratorSyntax &&
               usingEqualsValue.Parent.Parent?.Parent is UsingStatementSyntax;
    }

    static SyntaxNode SkipTransparentNodes(SyntaxNode node)
    {
        var current = node;
        while (current.Parent is ParenthesizedExpressionSyntax)
        {
            current = current.Parent;
        }

        return current;
    }
}
