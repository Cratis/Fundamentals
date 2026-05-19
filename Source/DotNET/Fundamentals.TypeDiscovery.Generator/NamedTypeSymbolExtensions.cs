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
    /// (i.e. it is not an error, implicit, anonymous, file-local, or private type,
    /// not a protected nested type, and not nested within a generic outer type).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type can be referenced; otherwise <see langword="false"/>.</returns>
    public static bool CanBeReferencedFromGeneratedCode(this INamedTypeSymbol type) =>
        type.TypeKind is not TypeKind.Error &&
        !type.IsImplicitlyDeclared &&
        !type.IsAnonymousType &&
        !type.IsFileLocal &&
        type.CanBeReferencedByName &&
        type.DeclaredAccessibility is not Accessibility.Private &&
        !type.IsProtectedNestedType() &&
        !type.IsNestedInGenericType();

    /// <summary>
    /// Returns whether the type is declared entirely within auto-generated source files
    /// produced by another source generator (files whose path ends with <c>.g.cs</c>).
    /// Such types must be excluded from the type-discovery output to prevent CS0436
    /// conflicts when the same type name is also imported from a referenced assembly.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if every declaring syntax reference is in a <c>.g.cs</c> file; otherwise <see langword="false"/>.</returns>
    public static bool IsFromSourceGenerator(this INamedTypeSymbol type) =>
        type.DeclaringSyntaxReferences.Length > 0 &&
        type.DeclaringSyntaxReferences.All(
            r => r.SyntaxTree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns whether the type is a protected nested type.
    /// Protected nested types are only accessible from subclasses of the containing type;
    /// generated code at namespace level cannot reference them, causing CS0122.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a protected nested type; otherwise <see langword="false"/>.</returns>
    public static bool IsProtectedNestedType(this INamedTypeSymbol type) =>
        type.ContainingType is not null &&
        type.DeclaredAccessibility is Accessibility.Protected or Accessibility.ProtectedAndFriend or Accessibility.ProtectedOrFriend;

    /// <summary>
    /// Returns whether the type is nested within a type that has unbound type parameters.
    /// The fully-qualified name of such a nested type includes the outer type's type parameter
    /// names, which are unresolvable symbols in generated code, causing CS0246.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if any containing type has type parameters; otherwise <see langword="false"/>.</returns>
    public static bool IsNestedInGenericType(this INamedTypeSymbol type)
    {
        var containing = type.ContainingType;

        while (containing is not null)
        {
            if (containing.Arity > 0)
            {
                return true;
            }

            containing = containing.ContainingType;
        }

        return false;
    }

    /// <summary>
    /// Returns whether the type is accessible from code generated into <paramref name="assembly"/>.
    /// Types from the same assembly may be <see langword="internal"/> or more visible.
    /// Types from external assemblies must be <see langword="public"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="assembly">The assembly into which generated code will be emitted.</param>
    /// <param name="globallyAccessibleAssemblyIdentities">
    /// Optional set of referenced assembly identities that are visible through <c>global::</c>.
    /// </param>
    /// <returns><see langword="true"/> if the type is accessible; otherwise <see langword="false"/>.</returns>
    public static bool IsAccessibleFromAssembly(
        this INamedTypeSymbol type,
        IAssemblySymbol assembly,
        HashSet<string>? globallyAccessibleAssemblyIdentities = null) =>
        SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, assembly)
            ? type.DeclaredAccessibility is not Accessibility.Private
            : type.DeclaredAccessibility is Accessibility.Public &&
              globallyAccessibleAssemblyIdentities?.Contains(type.ContainingAssembly.Identity.ToString()) != false;

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
    /// Excludes <c>System.Object</c>, the type itself, and any contracts that are not
    /// accessible from generated code emitted into <paramref name="currentAssembly"/>
    /// (i.e. internal types from external assemblies are excluded to prevent CS0122).
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="currentAssembly">The assembly into which generated code will be emitted.</param>
    /// <param name="globallyAccessibleAssemblyIdentities">
    /// Optional set of referenced assembly identities that are visible through <c>global::</c>.
    /// </param>
    /// <returns>The set of all contracts the type fulfils.</returns>
    public static IEnumerable<INamedTypeSymbol> GetAllBaseAndImplementingSymbols(
        this INamedTypeSymbol type,
        IAssemblySymbol currentAssembly,
        HashSet<string>? globallyAccessibleAssemblyIdentities = null) =>
        type.GetBaseTypes()
            .Concat(type.AllInterfaces)
            .SelectMany(GetThisAndMaybeOpenType)
            .Where(t => !SymbolEqualityComparer.Default.Equals(t, type) && t.SpecialType != SpecialType.System_Object)
            .Distinct(NamedTypeSymbolComparer.Instance)
            .Where(t => t.IsAccessibleFromAssembly(currentAssembly, globallyAccessibleAssemblyIdentities));

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
