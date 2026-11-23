// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Builds the C# source text for the generated <c language="csharp">GeneratedTypeDiscoveryProvider</c> class.
/// </summary>
internal static class GeneratedSourceBuilder
{
    /// <summary>
    /// Builds the full source text for the generated provider from the collected analysis results.
    /// </summary>
    /// <param name="namedTypes">All type-of expressions for types discovered in the assembly.</param>
    /// <param name="contractsAndImplementors">The contract-to-implementors map entries.</param>
    /// <param name="conventionServiceBindings">The I&lt;X&gt;/&lt;X&gt; convention bindings.</param>
    /// <param name="conventionSelfBindings">The self-binding type entries.</param>
    /// <param name="implementationSymbols">The discovered symbols used to select constructors to preserve.</param>
    /// <param name="experimentalDiagnosticIds">Diagnostic ids of [Experimental] types referenced by the generated code, to suppress.</param>
    /// <param name="enumTypes">Enum type expressions declared in the current assembly.</param>
    /// <returns>The complete C# source text.</returns>
    public static string Build(
        ImmutableArray<string> namedTypes,
        ImmutableArray<(string ContractExpression, ImmutableArray<string> ImplementorExpressions)> contractsAndImplementors,
        ImmutableArray<(string ServiceExpression, string ImplementationExpression, string LifetimeExpression)> conventionServiceBindings,
        ImmutableArray<(string ImplementationExpression, string LifetimeExpression)> conventionSelfBindings,
        IEnumerable<INamedTypeSymbol> implementationSymbols,
        ImmutableArray<string> experimentalDiagnosticIds = default,
        ImmutableArray<string> enumTypes = default)
        => BuildFromNames(
            namedTypes,
            contractsAndImplementors,
            conventionServiceBindings,
            conventionSelfBindings,
            ConstructorPreservation.GetImplementationTypes(
                implementationSymbols,
                conventionServiceBindings.Select(static binding => binding.ImplementationExpression)
                    .Concat(conventionSelfBindings.Select(static binding => binding.ImplementationExpression))),
            experimentalDiagnosticIds,
            enumTypes);

    /// <summary>Builds provider text from symbol-free discovery results.</summary>
    /// <param name="namedTypes">Discovered type expressions.</param>
    /// <param name="contractsAndImplementors">Contract-to-implementation mappings.</param>
    /// <param name="conventionServiceBindings">Convention service bindings.</param>
    /// <param name="conventionSelfBindings">Convention self bindings.</param>
    /// <param name="implementationsWithPublicConstructors">Types whose constructors are preserved.</param>
    /// <param name="experimentalDiagnosticIds">Experimental diagnostic identifiers.</param>
    /// <param name="enumTypes">Enum type expressions declared in the current assembly.</param>
    /// <param name="ownTypes">Expressions of the types declared in the current assembly. A binding whose implementation is not among them comes from a referenced package and is emitted guarded, so a package that removed the type cannot break the provider. When omitted, every type is treated as the assembly's own.</param>
    /// <returns>The complete provider source text.</returns>
    public static string BuildFromNames(
        ImmutableArray<string> namedTypes,
        ImmutableArray<(string ContractExpression, ImmutableArray<string> ImplementorExpressions)> contractsAndImplementors,
        ImmutableArray<(string ServiceExpression, string ImplementationExpression, string LifetimeExpression)> conventionServiceBindings,
        ImmutableArray<(string ImplementationExpression, string LifetimeExpression)> conventionSelfBindings,
        IEnumerable<string> implementationsWithPublicConstructors,
        ImmutableArray<string> experimentalDiagnosticIds,
        ImmutableArray<string> enumTypes = default,
        ISet<string>? ownTypes = null)
    {
        var hasEnums = !enumTypes.IsDefaultOrEmpty;
        var enumInterface = hasEnums ? ", global::Cratis.Json.ICanCreateEnumConverters" : string.Empty;
        string[] enumConverterMethod = [];
        if (hasEnums)
        {
            enumConverterMethod =
            [
                string.Empty,
                "    public global::System.Text.Json.Serialization.JsonConverter CreateEnumConverter(Type enumType)",
                "    {",
                .. enumTypes.Select(static name => $"        if (enumType == typeof({name})) return new global::Cratis.Json.EnumConverter<{name}>();"),
                "        return null;",
                "    }"
            ];
        }

        var experimentalIds = experimentalDiagnosticIds.IsDefault ? [] : experimentalDiagnosticIds;
        var suppressedDiagnostics = string.Join(", ", ["CS0612", "CS0618", .. experimentalIds]);
        var definedTypes = string.Join(",\n", namedTypes.Select(static name => $"        typeof({name})"));

        bool IsOwn(string expression) => ownTypes?.Contains(expression) != false;
        var serviceBindings = conventionServiceBindings.Select(b => (Entry: GetServiceBinding(b), IsOwn: IsOwn(b.ImplementationExpression))).ToArray();
        var selfBindings = conventionSelfBindings.Select(b => (Entry: GetSelfBinding(b), IsOwn: IsOwn(b.ImplementationExpression))).ToArray();

        var serviceBindingsProperty = GetBindingsProperty("global::Cratis.DependencyInjection.ConventionServiceBinding", "ConventionServiceBindings", serviceBindings);
        var selfBindingsProperty = GetBindingsProperty("global::Cratis.DependencyInjection.ConventionSelfBinding", "SelfBindings", selfBindings);
        var tryAddMethod = serviceBindings.Concat(selfBindings).Any(static binding => !binding.IsOwn) ? GetTryAddMethod() : [];

        return string.Join(
            "\n",
            [
                "// <auto-generated />",
                string.Empty,

                // Discovery lists every type it finds, and a package is free to mark one of its types [Obsolete] -
                // a typeof of it here is then an obsolete-member warning, an error in any project that treats
                // warnings as errors, in code nobody can edit or suppress by path. Listing a type is not using it.
                // Types marked [Experimental("ID")] likewise raise ID wherever they are referenced.
                $"#pragma warning disable {suppressedDiagnostics}",
                string.Empty,
                "namespace Cratis.Types.Generated;",
                string.Empty,
                "using System;",
                "using System.Collections.Generic;",
                "using System.Linq;",
                "using System.Reflection;",
                "using System.Runtime.CompilerServices;",
                string.Empty,
                $"file sealed class GeneratedTypeDiscoveryProvider : global::Cratis.Types.ICanProvideAssembliesForDiscovery, global::Cratis.Types.ICanProvideContractToImplementorsForDiscovery, global::Cratis.DependencyInjection.ICanProvideConventionsForDependencyInjection{enumInterface}",
                "{",
                "    public IEnumerable<Assembly> Assemblies => [typeof(GeneratedTypeDiscoveryProvider).Assembly];",
                string.Empty,
                "    public IEnumerable<Type> DefinedTypes =>",
                "    [",
                definedTypes,
                "    ];",
                string.Empty,
                ..serviceBindingsProperty,
                string.Empty,
                ..selfBindingsProperty,
                string.Empty,
                "    public IDictionary<Type, IEnumerable<Type>> ContractsAndImplementors => new Dictionary<Type, IEnumerable<Type>>",
                "    {",
                ..contractsAndImplementors.Select(GetContractAndImplementorsMapEntry),
                "    };",
                string.Empty,
                "    public void Initialize()",
                "    {",
                "    }",
                ..enumConverterMethod,
                ..tryAddMethod,
                "}",
                string.Empty,
                "file static class GeneratedTypeDiscoveryProviderRegistration",
                "{",
                "    [ModuleInitializer]",
                ..implementationsWithPublicConstructors.Select(static implementation =>
                    $"    [global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors, typeof({implementation}))]"),
                "    public static void Register() =>",
                "        global::Cratis.Types.GeneratedTypeDiscoveryRegistry.Register(new GeneratedTypeDiscoveryProvider());",
                "}"
            ]);
    }

    /// <summary>
    /// A typeof() is resolved when the method holding it is compiled, so a package type that a newer package
    /// version removed would fail the whole property. Each package binding lives in its own delegate and is
    /// skipped, by this helper, when its type no longer loads.
    /// </summary>
    /// <returns>The lines of the generated helper method.</returns>
    static string[] GetTryAddMethod() =>
    [
        string.Empty,
        "    static void TryAdd<T>(List<T> into, Func<T> create)",
        "    {",
        "        try",
        "        {",
        "            into.Add(create());",
        "        }",
        "        catch (Exception exception) when (exception is TypeLoadException or global::System.IO.FileNotFoundException or global::System.IO.FileLoadException or MissingMemberException)",
        "        {",
        "        }",
        "    }"
    ];

    static string GetServiceBinding((string ServiceExpression, string ImplementationExpression, string LifetimeExpression) binding) =>
        $"new global::Cratis.DependencyInjection.ConventionServiceBinding(typeof({binding.ServiceExpression}), typeof({binding.ImplementationExpression}), {binding.LifetimeExpression})";

    static string GetSelfBinding((string ImplementationExpression, string LifetimeExpression) binding) =>
        $"new global::Cratis.DependencyInjection.ConventionSelfBinding(typeof({binding.ImplementationExpression}), {binding.LifetimeExpression})";

    static string[] GetBindingsProperty(string bindingType, string name, (string Entry, bool IsOwn)[] bindings)
    {
        if (bindings.All(static binding => binding.IsOwn))
        {
            return
            [
                $"    public IEnumerable<{bindingType}> {name} =>",
                "    [",
                string.Join(",\n", bindings.Select(static binding => $"        {binding.Entry}")),
                "    ];"
            ];
        }

        // Entries are emitted in their sorted position so that registration order, which decides which
        // implementation wins for a service, is the same as when every binding is the assembly's own.
        return
        [
            $"    public IEnumerable<{bindingType}> {name}",
            "    {",
            "        get",
            "        {",
            $"            List<{bindingType}> bindings = [];",
            ..bindings.Select(static binding => binding.IsOwn
                ? $"            bindings.Add({binding.Entry});"
                : $"            TryAdd(bindings, static () => {binding.Entry});"),
            "            return bindings;",
            "        }",
            "    }"
        ];
    }

    static string GetContractAndImplementorsMapEntry((string ContractExpression, ImmutableArray<string> ImplementorExpressions) entry) =>
        $"        [typeof({entry.ContractExpression})] = [{string.Join(", ", entry.ImplementorExpressions.Select(static i => $"typeof({i})"))}],";
}
