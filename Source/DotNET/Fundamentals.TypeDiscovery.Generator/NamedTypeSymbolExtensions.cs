// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Extension methods for <see cref="INamedTypeSymbol"/> that classify and describe types
/// for the purposes of type-discovery and DI-convention analysis.
/// </summary>
internal static class NamedTypeSymbolExtensions
{
    const string IgnoreConventionAttributeFullName = "Cratis.DependencyInjection.IgnoreConventionAttribute";
    const string SingletonAttributeFullName = "Cratis.DependencyInjection.SingletonAttribute";
    const string ScopedAttributeFullName = "Cratis.DependencyInjection.ScopedAttribute";

    /// <summary>
    /// Returns whether the type can safely be referenced from generated code
    /// (i.e. it is not an error, implicit, anonymous, or private type).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type can be referenced; otherwise <see langword="false"/>.</returns>
    public static bool CanBeReferencedFromGeneratedCode(this INamedTypeSymbol type) =>
        type.TypeKind is not TypeKind.Error &&
        !type.IsImplicitlyDeclared &&
        !type.IsAnonymousType &&
        type.CanBeReferencedByName &&
        type.DeclaredAccessibility is not Accessibility.Private;

    /// <summary>
    /// Returns whether the type is a concrete implementation — not an interface or abstract class.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a concrete implementation; otherwise <see langword="false"/>.</returns>
    public static bool IsImplementation(this INamedTypeSymbol type) =>
        type.TypeKind is not TypeKind.Interface &&
        !type.IsAbstract &&
        type.Arity == 0;

    /// <summary>
    /// Returns the C# <c>typeof()</c> argument expression for this type.
    /// Generic types are represented in their unbound open-generic form: <c>global::Foo.Bar&lt;&gt;</c>.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    /// <returns>A fully-qualified C# type expression string.</returns>
    public static string GetTypeOfExpression(this INamedTypeSymbol type)
    {
        var symbol = type.Arity > 0 ? type.ConstructUnboundGenericType() : type;
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    /// <summary>
    /// Returns whether the type inherits from <c>System.Exception</c>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an exception type; otherwise <see langword="false"/>.</returns>
    public static bool IsExceptionType(this INamedTypeSymbol type)
    {
        var current = type.BaseType;

        while (current is not null)
        {
            if (current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Exception")
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Returns whether the type carries the <c>[IgnoreConvention]</c> attribute
    /// and should therefore be excluded from all DI convention discovery.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type should be ignored; otherwise <see langword="false"/>.</returns>
    public static bool ShouldIgnoreConvention(this INamedTypeSymbol type) =>
        type.GetAttributes().Any(_ => _.AttributeClass?.ToDisplayString() == IgnoreConventionAttributeFullName);

    /// <summary>
    /// Returns whether the type lives in a <c>System.*</c> or <c>Microsoft.*</c> namespace
    /// and should therefore be excluded from DI self-binding convention discovery.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is in a system namespace; otherwise <see langword="false"/>.</returns>
    public static bool IsInSystemOrMicrosoftNamespace(this INamedTypeSymbol type)
    {
        var ns = type.ContainingNamespace.ToDisplayString();
        return ns.StartsWith("System") || ns.StartsWith("Microsoft");
    }

    /// <summary>
    /// Returns whether any public constructor has a parameter whose type is primitive-like
    /// and therefore cannot be resolved from a DI container.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if any public constructor has an unresolvable parameter; otherwise <see langword="false"/>.</returns>
    public static bool HasConstructorWithUnresolvableParameters(this INamedTypeSymbol type) =>
        type.InstanceConstructors
            .Any(ctor => ctor.DeclaredAccessibility == Accessibility.Public &&
                         ctor.Parameters.Any(p => p.Type.IsPrimitiveLike()));

    /// <summary>
    /// Returns whether any public constructor has a parameter whose type is a record.
    /// Records are excluded from self-binding because they typically represent data, not services.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if any public constructor has a record parameter; otherwise <see langword="false"/>.</returns>
    public static bool HasConstructorWithRecordTypes(this INamedTypeSymbol type) =>
        type.InstanceConstructors
            .Any(ctor => ctor.DeclaredAccessibility == Accessibility.Public &&
                         ctor.Parameters.Any(p => p.Type is INamedTypeSymbol n && n.IsRecord));

    /// <summary>
    /// Returns the DI service lifetime expression for the type, based on <c>[Singleton]</c>
    /// or <c>[Scoped]</c> attributes. Defaults to <c>Transient</c>.
    /// </summary>
    /// <param name="type">The type to classify.</param>
    /// <returns>A fully-qualified <c>ServiceLifetime</c> expression string.</returns>
    public static string GetServiceLifetimeExpression(this INamedTypeSymbol type)
    {
        if (type.GetAttributes().Any(_ => _.AttributeClass?.ToDisplayString() == SingletonAttributeFullName))
        {
            return "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton";
        }

        if (type.GetAttributes().Any(_ => _.AttributeClass?.ToDisplayString() == ScopedAttributeFullName))
        {
            return "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped";
        }

        return "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient";
    }

    /// <summary>
    /// Returns all base types and implemented interface types for the given type,
    /// including their open-generic forms where applicable.
    /// Excludes <c>System.Object</c> and the type itself.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The set of all contracts the type fulfils.</returns>
    public static IEnumerable<INamedTypeSymbol> GetAllBaseAndImplementingSymbols(this INamedTypeSymbol type) =>
        type.GetBaseTypes()
            .Concat(type.AllInterfaces)
            .SelectMany(GetThisAndMaybeOpenType)
            .Where(t => !SymbolEqualityComparer.Default.Equals(t, type) && t.SpecialType != SpecialType.System_Object)
            .Distinct(NamedTypeSymbolComparer.Instance)
            .Where(t => t.CanBeReferencedFromGeneratedCode());

    /// <summary>
    /// Enumerates the type itself and all of its base types up the inheritance chain.
    /// </summary>
    /// <param name="type">The type to walk.</param>
    /// <returns>The type and each of its base types in order.</returns>
    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this INamedTypeSymbol type)
    {
        var current = type;

        while (current is not null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

    static IEnumerable<INamedTypeSymbol> GetThisAndMaybeOpenType(INamedTypeSymbol symbol)
    {
        yield return symbol;

        if (symbol.IsGenericType && !symbol.IsUnboundGenericType)
        {
            yield return symbol.ConstructUnboundGenericType();
        }
    }
}
