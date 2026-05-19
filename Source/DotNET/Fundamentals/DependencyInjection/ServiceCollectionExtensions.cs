// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cratis.Reflection;
using Cratis.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    static readonly string[] _namespacesToIgnoreForSelfBinding = ["System", "Microsoft"];

    /// <summary>
    /// Add service bindings by convention.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <returns><see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddBindingsByConvention(this IServiceCollection services)
    {
        return TryAddGeneratedBindingsByConvention(services)
            ? services
            : AddBindingsByConventionUsingReflectionFallback(services);
    }

    /// <summary>
    /// Add self bindings for types that are not already registered.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <returns><see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddSelfBindings(this IServiceCollection services)
    {
        return TryAddGeneratedSelfBindings(services)
            ? services
            : AddSelfBindingsUsingReflectionFallback(services);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Generated convention metadata drives registration and constructors are preserved by generated usage in AOT scenarios.")]
    static bool TryAddGeneratedBindingsByConvention(IServiceCollection services)
    {
        EnsureGeneratedTypeDiscoveryProvidersAreRegistered();

        var generatedBindings = GeneratedTypeDiscoveryRegistry.Providers
            .OfType<ICanProvideConventionsForDependencyInjection>()
            .SelectMany(_ => _.ConventionServiceBindings)
            .ToArray();

        if (generatedBindings.Length == 0)
        {
            return false;
        }

        generatedBindings.ToList().ForEach(binding =>
        {
            if (services.Any(_ => _.ServiceType == binding.ServiceType) || binding.ImplementationType.IsAbstract)
            {
                return;
            }

            _ = binding.Lifetime switch
            {
                ServiceLifetime.Singleton => services.AddSingleton(binding.ServiceType, binding.ImplementationType),
                ServiceLifetime.Scoped => services.AddScoped(binding.ServiceType, binding.ImplementationType),
                _ => services.AddTransient(binding.ServiceType, binding.ImplementationType)
            };
        });

        return true;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Generated convention metadata drives registration and constructors are preserved by generated usage in AOT scenarios.")]
    static bool TryAddGeneratedSelfBindings(IServiceCollection services)
    {
        EnsureGeneratedTypeDiscoveryProvidersAreRegistered();

        var generatedBindings = GeneratedTypeDiscoveryRegistry.Providers
            .OfType<ICanProvideConventionsForDependencyInjection>()
            .SelectMany(_ => _.SelfBindings)
            .ToArray();

        if (generatedBindings.Length == 0)
        {
            return false;
        }

        generatedBindings.ToList().ForEach(binding =>
        {
            if (services.Any(s => s.ServiceType == binding.ImplementationType))
            {
                return;
            }

            _ = binding.Lifetime switch
            {
                ServiceLifetime.Singleton => services.AddSingleton(binding.ImplementationType, binding.ImplementationType),
                ServiceLifetime.Scoped => services.AddScoped(binding.ImplementationType, binding.ImplementationType),
                _ => services.AddTransient(binding.ImplementationType, binding.ImplementationType)
            };
        });

        return true;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection-based convention scanning is a compatibility fallback when generated metadata is unavailable.")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Reflection-based convention scanning is a compatibility fallback when generated metadata is unavailable.")]
    static IServiceCollection AddBindingsByConventionUsingReflectionFallback(IServiceCollection services)
    {
        var types = Types.Types.Instance;

        static bool convention(Type i, Type t) => i.Namespace == t.Namespace && i.Name == $"I{t.Name}";

        var conventionBasedTypes = types.All
            .Where(_ =>
            {
                if (ShouldIgnoreConvention(_))
                {
                    return false;
                }

                var interfaces = _.GetInterfaces();
                if (interfaces.Length > 0)
                {
                    var conventionInterface = interfaces.SingleOrDefault(i => convention(i, _));
                    if (conventionInterface != default)
                    {
                        return types.All.Count(type => type.HasInterface(conventionInterface)) == 1;
                    }
                }
                return false;
            });

        foreach (var conventionBasedType in conventionBasedTypes)
        {
            var interfaceToBind = types.All.First(_ => _.IsInterface && convention(_, conventionBasedType));
            if (services.Any(_ => _.ServiceType == interfaceToBind) || conventionBasedType.IsAbstract)
            {
                continue;
            }

            _ = GetServiceLifetime(conventionBasedType) switch
            {
                ServiceLifetime.Singleton => services.AddSingleton(interfaceToBind, conventionBasedType),
                ServiceLifetime.Scoped => services.AddScoped(interfaceToBind, conventionBasedType),
                _ => services.AddTransient(interfaceToBind, conventionBasedType)
            };
        }

        return services;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Reflection-based convention scanning is a compatibility fallback when generated metadata is unavailable.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection-based convention scanning is a compatibility fallback when generated metadata is unavailable.")]
    static IServiceCollection AddSelfBindingsUsingReflectionFallback(IServiceCollection services)
    {
        const TypeAttributes staticType = TypeAttributes.Abstract | TypeAttributes.Sealed;

        Types.Types.Instance.All.Where(_ =>
            (_.Attributes & staticType) != staticType &&
            !_.IsInterface &&
            !_.IsAbstract &&
            !ShouldIgnoreConvention(_) &&
            !ShouldIgnoreNamespace(_.Namespace ?? string.Empty) &&
            !HasConstructorWithUnresolvableParameters(_) &&
            !HasConstructorWithRecordTypes(_) &&
            !_.IsAssignableTo(typeof(Exception)) &&
            !services.Any(s => s.ServiceType == _)).ToList().ForEach(_ =>
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var __ = GetServiceLifetime(_) switch
            {
                ServiceLifetime.Singleton => services.AddSingleton(_, _),
                ServiceLifetime.Scoped => services.AddScoped(_, _),
                _ => services.AddTransient(_, _)
            };
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        });

        return services;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection-based convention scanning is a compatibility fallback when generated metadata is unavailable.")]
    static bool ShouldIgnoreConvention(Type type) =>
        type.HasAttribute<IgnoreConventionAttribute>();

    static bool ShouldIgnoreNamespace(string namespaceToCheck) =>
        _namespacesToIgnoreForSelfBinding.Any(namespaceToCheck.StartsWith);

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection-based convention scanning is a compatibility fallback when generated metadata is unavailable.")]
    static bool HasConstructorWithUnresolvableParameters(Type type) =>
        type.GetConstructors().Any(_ => _.GetParameters().Any(p => p.ParameterType.IsAPrimitiveType()));

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection-based convention scanning is a compatibility fallback when generated metadata is unavailable.")]
    static bool HasConstructorWithRecordTypes(Type type) =>
        type.GetConstructors().Any(_ => _.GetParameters().Any(p => p.ParameterType.IsRecord()));

    static ServiceLifetime GetServiceLifetime(Type type)
    {
        if (type.HasAttribute<SingletonAttribute>())
        {
            return ServiceLifetime.Singleton;
        }

        if (type.HasAttribute<ScopedAttribute>())
        {
            return ServiceLifetime.Scoped;
        }

        return ServiceLifetime.Transient;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Referenced assemblies must be visited to run module initializers that register generated providers.")]
    static void EnsureGeneratedTypeDiscoveryProvidersAreRegistered()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblies = new HashSet<Assembly>(loadedAssemblies);
        var visitedAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var assemblyNamesToLoad = new Queue<AssemblyName>();

        void EnqueueReferencesFor(Assembly assembly)
        {
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies().Where(_ => visitedAssemblyNames.Add(_.FullName)))
            {
                assemblyNamesToLoad.Enqueue(referencedAssemblyName);
            }
        }

        foreach (var loadedAssembly in loadedAssemblies)
        {
            EnqueueReferencesFor(loadedAssembly);
        }
        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            _ = assemblies.Add(entryAssembly);
            EnqueueReferencesFor(entryAssembly);
        }

        while (assemblyNamesToLoad.TryDequeue(out var assemblyName))
        {
            var assembly = assemblies.SingleOrDefault(_ => AssemblyName.ReferenceMatchesDefinition(_.GetName(), assemblyName));

            if (assembly is null)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                catch (FileLoadException)
                {
                    continue;
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }

            if (assemblies.Add(assembly))
            {
                EnqueueReferencesFor(assembly);
            }
        }

        foreach (var assembly in assemblies.Where(static _ => !_.IsDynamic))
        {
            RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);
        }
    }
}
