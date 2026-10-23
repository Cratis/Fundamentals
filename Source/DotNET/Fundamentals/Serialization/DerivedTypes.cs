// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cratis.DependencyInjection;
using Cratis.Reflection;
using Cratis.Types;

namespace Cratis.Serialization;

/// <summary>
/// Represents an implementation of <see cref="IDerivedTypes"/>.
/// </summary>
[Singleton]
public class DerivedTypes : IDerivedTypes
{
    /// <summary>
    /// Gets the global instance of <see cref="DerivedTypes"/>.
    /// </summary>
    /// <remarks>
    /// Its recommended to use the singleton defined here, rather than building your own instance.
    /// This is due to the performance impact of scanning all assemblies in the application.
    /// </remarks>
    public static readonly DerivedTypes Instance = new(Types.Types.Instance);

    static readonly ConditionalWeakTable<ITypes, DerivedTypes> _perUniverse = CreateUniverseTable();

    readonly Dictionary<Type, DerivedTypeAndIdentifier[]> _targetTypeToDerivedType;
    readonly Dictionary<(Type TargetType, DerivedTypeId Identifier), Type> _derivedTypeByIdentifier;
    readonly Dictionary<Type, Type> _derivedTypeToTargetType;

    /// <summary>
    /// Initializes a new instance of <see cref="ITypes"/>.
    /// </summary>
    /// <param name="types"><see cref="ITypes"/> representing all types in the system.</param>
    public DerivedTypes(ITypes types)
    {
        var derivedTypes = types.All.Where(_ => _.HasAttribute<DerivedTypeAttribute>()).ToArray();
        ThrowIfAmbiguousDerivedTypeIdentifiers(derivedTypes);

        var registrations = derivedTypes
            .SelectMany(GetRegistrationsFrom)
            .ToArray();

        _targetTypeToDerivedType = registrations
            .GroupBy(_ => _.TargetType)
            .ToDictionary(
                _ => _.Key,
                _ => _.GroupBy(registration => registration.DerivedType)
                      .Select(group => new DerivedTypeAndIdentifier(group.Key, _.Key, group.First().Identifier))
                      .ToArray());

        _derivedTypeByIdentifier = _targetTypeToDerivedType
            .SelectMany(_ => _.Value)
            .ToDictionary(_ => (_.TargetType, _.Identifier), _ => _.DerivedType);

        _derivedTypeToTargetType = registrations
            .Where(_ => _.IsPrimaryTarget)
            .ToDictionary(_ => _.DerivedType, _ => _.TargetType);
    }

    /// <inheritdoc/>
    public IEnumerable<Type> TypesWithDerivatives => _targetTypeToDerivedType.Keys;

    /// <summary>
    /// Gets the <see cref="DerivedTypes"/> for a type universe, reusing one instance per <see cref="ITypes"/> instance.
    /// </summary>
    /// <param name="types"><see cref="ITypes"/> representing all types in the system.</param>
    /// <returns>The shared <see cref="DerivedTypes"/> for the given universe.</returns>
    /// <remarks>
    /// The universe is held weakly, so it can be collected when nothing else references it.
    /// </remarks>
    public static DerivedTypes For(ITypes types) => _perUniverse.GetValue(types, static universe => new DerivedTypes(universe));

    /// <inheritdoc/>
    public Type GetDerivedTypeFor(Type targetType, DerivedTypeId derivedTypeId)
    {
        if (!_derivedTypeByIdentifier.TryGetValue((targetType, derivedTypeId), out var derivedType))
        {
            throw new MissingDerivedTypeForTargetType(targetType, derivedTypeId);
        }

        return derivedType;
    }

    /// <inheritdoc/>
    public Type GetTargetTypeFor(Type derivedType)
    {
        if (!_derivedTypeToTargetType.TryGetValue(derivedType, out var value))
        {
            throw new MissingTargetTypeForDerivedType(derivedType);
        }

        return value;
    }

    /// <inheritdoc/>
    public bool IsDerivedType(Type type) => _derivedTypeToTargetType.ContainsKey(type);

    /// <inheritdoc/>
    public bool HasDerivatives(Type type) => _targetTypeToDerivedType.ContainsKey(type);

#pragma warning disable IDE0028 // ConditionalWeakTable has no single-argument Add, so a collection expression is not possible.
    static ConditionalWeakTable<ITypes, DerivedTypes> CreateUniverseTable() => new();
#pragma warning restore IDE0028

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Derived type interface and inheritance inspection is retained for compatibility fallback behavior.")]
    IEnumerable<Registration> GetRegistrationsFrom(Type derivedType)
    {
        var attribute = derivedType.GetCustomAttribute<DerivedTypeAttribute>()!;
        var primaryTargetType = attribute.TargetType;

        if (primaryTargetType is not null)
        {
            var interfaces = derivedType.GetInterfaces().Where(_ => !_.Namespace?.StartsWith("System") ?? true).ToArray();
            ThrowIfMissingTargetTypeForDerivedType(derivedType, interfaces);
            ThrowIfTargetTypeMismatchesForDerivedType(derivedType, primaryTargetType);
        }
        else
        {
            var interfaces = derivedType.GetInterfaces().Where(_ => !_.Namespace?.StartsWith("System") ?? true).ToArray();
            ThrowIfAmbiguousTargetTypeForDerivedType(derivedType, interfaces);
            ThrowIfMissingTargetTypeForDerivedType(derivedType, interfaces);
            primaryTargetType = interfaces[0];
        }

        var registrations = new List<Registration>
        {
            new(derivedType, primaryTargetType, attribute.Identifier, true)
        };

        var currentBaseType = derivedType.BaseType;
        while (currentBaseType is not null && currentBaseType != typeof(object))
        {
            if (!currentBaseType.Namespace?.StartsWith("System") ?? true)
            {
                registrations.Add(new(derivedType, currentBaseType, attribute.Identifier, false));
            }

            currentBaseType = currentBaseType.BaseType;
        }

        return registrations;
    }

    void ThrowIfAmbiguousDerivedTypeIdentifiers(IEnumerable<Type> derivedTypes)
    {
        var groupedByIdentifier = derivedTypes.GroupBy(_ => _.GetCustomAttribute<DerivedTypeAttribute>()!.Identifier);
        var withMultipleIdentifiers = groupedByIdentifier.Where(_ => _.Count() > 1);
        if (withMultipleIdentifiers.Any())
        {
            throw new AmbiguousDerivedTypeIdentifiers(withMultipleIdentifiers.First().AsEnumerable());
        }
    }

    void ThrowIfAmbiguousTargetTypeForDerivedType(Type derivedType, Type[] interfaces)
    {
        if (interfaces.Length > 1)
        {
            throw new AmbiguousTargetTypeForDerivedType(derivedType);
        }
    }

    void ThrowIfMissingTargetTypeForDerivedType(Type derivedType, Type[] interfaces)
    {
        if (interfaces.Length == 0)
        {
            throw new MissingTargetTypeForDerivedType(derivedType);
        }
    }

    void ThrowIfTargetTypeMismatchesForDerivedType(Type derivedType, Type targetType)
    {
        if (!derivedType.IsAssignableTo(targetType))
        {
            throw new TargetTypeMismatchForDerivedType(derivedType);
        }
    }

    sealed record Registration(Type DerivedType, Type TargetType, DerivedTypeId Identifier, bool IsPrimaryTarget);
    sealed record DerivedTypeAndIdentifier(Type DerivedType, Type TargetType, DerivedTypeId Identifier);
}
