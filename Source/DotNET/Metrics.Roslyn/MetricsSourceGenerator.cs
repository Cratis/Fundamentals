// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cratis.Metrics.Roslyn.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cratis.Metrics.Roslyn;

/// <summary>
/// Represents the source generator for metrics.
/// </summary>
[Generator]
public class MetricsSourceGenerator : IIncrementalGenerator
{
    static readonly string[] _systemUsings =
    [
        "System.Diagnostics",
        "System.Diagnostics.Metrics"
    ];

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter to partial static classes with partial static methods that have metrics attributes
        var candidateClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateClass(node),
                transform: static (ctx, _) => GetClassDeclaration(ctx))
            .Where(static candidate => candidate is not null);

        // Combine with compilation to get semantic model
        var compilationAndClasses = context.CompilationProvider.Combine(candidateClasses.Collect());

        // Generate source for each class
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(spc, source.Left, source.Right!));
    }

    static bool IsCandidateClass(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classSyntax) return false;

        if (!classSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)) return false;
        if (!classSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)) return false;

        return classSyntax.Members.Any(member =>
            member is MethodDeclarationSyntax method &&
            method.Modifiers.Any(SyntaxKind.PartialKeyword) &&
            method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
            HasMetricsAttribute(method));
    }

    static bool HasMetricsAttribute(MethodDeclarationSyntax method)
    {
        var metricsAttributes = new[] { "Counter", "Gauge" };
        return method.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attr => metricsAttributes.Any(m => attr.Name.ToString().StartsWith(m)));
    }

    static ClassDeclarationSyntax? GetClassDeclaration(GeneratorSyntaxContext context)
    {
        return context.Node as ClassDeclarationSyntax;
    }

    static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> candidates)
    {
        if (candidates.IsDefaultOrEmpty) return;

        var counterAttribute = compilation.GetTypeByMetadataName("Cratis.Metrics.CounterAttribute`1");
        var gaugeAttribute = compilation.GetTypeByMetadataName("Cratis.Metrics.GaugeAttribute`1");

        if (counterAttribute is null || gaugeAttribute is null) return;

        foreach (var candidate in candidates)
        {
            if (candidate is null) continue;

            var classDefinition = $"{candidate.Modifiers} class {candidate.Identifier.ValueText}";
            var usings = GetUsingsFor(candidate);

            var templateData = new MetricsTemplateData
            {
                Namespace = (candidate.Parent as BaseNamespaceDeclarationSyntax)!.Name.ToString(),
                ClassName = candidate.Identifier.ValueText,
                ClassDefinition = classDefinition,
                UsingStatements = [.. usings]
            };

            var semanticModel = compilation.GetSemanticModel(candidate.SyntaxTree);
            foreach (var member in candidate.Members)
            {
                if (member is not MethodDeclarationSyntax method) continue;

                var methodSymbol = semanticModel.GetDeclaredSymbol(method);
                if (methodSymbol is not null)
                {
                    var attributes = methodSymbol.GetAttributes();
                    var methodSignature = $"{method.Modifiers} {method.ReturnType} {method.Identifier.ValueText}({method.ParameterList.Parameters})";

                    ValidateFirstParameter(context, method, methodSignature);

                    var scopeParameter = method.ParameterList.Parameters[0].Identifier.ValueText;

                    var type = method.ParameterList.Parameters[0].Type;
                    var isScoped = type?.ToString().StartsWith("IMeterScope") ?? false;

                    AddMetricIfAny(context, templateData.Counters, counterAttribute, method, methodSignature, attributes, isScoped, scopeParameter);
                    AddMetricIfAny(context, templateData.Gauges, gaugeAttribute, method, methodSignature, attributes, isScoped, scopeParameter, true);
                }
            }

            if (templateData.Counters.Count > 0 ||
                templateData.Gauges.Count > 0)
            {
                var source = TemplateTypes.Metrics(templateData);
                context.AddSource($"{candidate.Identifier.ValueText}.g.cs", source);
            }
        }
    }

    static IEnumerable<string> GetUsingsFor(ClassDeclarationSyntax candidate)
    {
        var current = candidate.Parent;

        if (current is null)
        {
            return [];
        }

        var usings = new List<string>();

        for (; ; )
        {
            usings.AddRange(current.DescendantNodes().OfType<UsingDirectiveSyntax>().Select(_ => _.Name?.ToString() ?? string.Empty));
            if (current is CompilationUnitSyntax)
            {
                break;
            }
            current = current.Parent;
            if (current is null)
            {
                break;
            }
        }

        foreach (var usingNamespace in _systemUsings.Reverse())
        {
            if (!usings.Contains(usingNamespace))
            {
                usings.Insert(0, usingNamespace);
            }
        }

        return usings.Distinct();
    }

    static IEnumerable<ParameterSyntax> GetActualParameters(MethodDeclarationSyntax method) =>
        method.ParameterList.Parameters.Skip(1);

    static IEnumerable<TagTemplateData> GetParametersAsTags(IEnumerable<ParameterSyntax> parameters) =>
        parameters.Select(parameter => new TagTemplateData
        {
            Name = parameter.Identifier.ValueText,
            Type = parameter.Type!.ToString()
        });

    static void ValidateFirstParameter(SourceProductionContext context, MethodDeclarationSyntax method, string methodSignature)
    {
        if (method.ParameterList.Parameters.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "METRICS001",
                    "Missing required first parameter representing the type being extended",
                    "Method '{0}' is missing a value parameter",
                    "Metrics",
                    DiagnosticSeverity.Error,
                    true),
                method.GetLocation(),
                methodSignature));

            return;
        }

        var parameterType = method.ParameterList.Parameters[0].Type?.ToString() ?? string.Empty;

        if (!parameterType.StartsWith("IMeterScope") && !parameterType.StartsWith("IMeter"))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "METRICS002",
                    "First parameter must be either an IMeter<> or IMeterScope<>",
                    "Method '{0}' must either use IMeter<> or IMeterScope<> as the first parameter",
                    "Metrics",
                    DiagnosticSeverity.Error,
                    true),
                method.GetLocation(),
                methodSignature));
        }
    }

    static void ValidateValueParameter(SourceProductionContext context, MethodDeclarationSyntax method, string methodSignature, string valueParameter, bool valueParameterRequired)
    {
        if (valueParameter.Length == 0 && valueParameterRequired)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "METRICS0013",
                    "Missing required value parameter",
                    "Method '{0}' is missing a value parameter",
                    "Metrics",
                    DiagnosticSeverity.Error,
                    true),
                method.GetLocation(),
                methodSignature));
        }
    }

    static void AddMetricIfAny(
        SourceProductionContext context,
        IList<MetricTemplateData> metrics,
        INamedTypeSymbol attributeToLookFor,
        MethodDeclarationSyntax method,
        string methodSignature,
        ImmutableArray<AttributeData> attributes,
        bool isScoped,
        string scopeParameter,
        bool valueParameterRequired = false)
    {
        var attribute = attributes.FirstOrDefault(_ => SymbolEqualityComparer.Default.Equals(_.AttributeClass?.OriginalDefinition, attributeToLookFor));
        if (attribute is not null)
        {
            var type = attribute.AttributeClass!.TypeArguments[0].ToString();

            var parameters = GetActualParameters(method);
            var valueParameter = parameters.FirstOrDefault(p => p.Type?.ToString() == type)?.Identifier.ValueText ?? string.Empty;
            var hasValueParameter = valueParameter.Length > 0;
            ValidateValueParameter(context, method, methodSignature, valueParameter, valueParameterRequired);
            if (hasValueParameter)
            {
                parameters = parameters.Where(p => p.Identifier.ValueText != valueParameter);
            }
            var tags = GetParametersAsTags(parameters);
            var name = attribute.ConstructorArguments[0].Value!.ToString();
            var description = attribute.ConstructorArguments[1].Value!.ToString();
            metrics.Add(
                    new MetricTemplateData
                    {
                        Name = name,
                        Description = description,
                        Type = type,
                        MethodName = method.Identifier.ValueText,
                        MethodSignature = methodSignature,
                        IsScoped = isScoped,
                        ScopeParameter = scopeParameter,
                        ValueParameter = valueParameter,
                        HasValueParameter = hasValueParameter,
                        Tags = tags
                    });
        }
    }
}
