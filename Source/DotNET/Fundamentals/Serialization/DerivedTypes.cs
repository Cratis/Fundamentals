// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
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

    readonly Dictionary<Type, IEnumerable<DerivedTypeAndIdentifier>> _targetTypeToDerivedType;
    readonly Dictionary<Type, Type> _derivedTypeToTargetType;

    /// <summary>
    /// Initializes a new instance of <see cref="ITypes"/>.
    /// </summary>
    /// <param name="types"><see cref="ITypes"/> representing all types in the system.</param>
    public DerivedTypes(ITypes types)
    {
        var derivedTypes = types.All.Where(_ => _.HasAttribute<DerivedTypeAttribute>());
        ThrowIfAmbiguousDerivedTypeIdentifiers(derivedTypes);

        _targetTypeToDerivedType = derivedTypes
            .GroupBy(GetTargetTypeFrom)
            .ToDictionary(_ => _.Key, _ => _.Select(dt => new DerivedTypeAndIdentifier(dt, _.Key, dt.GetCustomAttribute<DerivedTypeAttribute>()!.Identifier)));

        _derivedTypeToTargetType = _targetTypeToDerivedType
            .SelectMany(_ => _.Value)
            .ToDictionary(_ => _.DerivedType, _ => _.TargetType);
    }

    /// <inheritdoc/>
    public IEnumerable<Type> TypesWithDerivatives => _targetTypeToDerivedType.Keys;

    /// <inheritdoc/>
    public Type GetDerivedTypeFor(Type targetType, DerivedTypeId derivedTypeId)
    {
        ThrowIfMissingDerivedTypeOrMissingIdentifier(targetType, derivedTypeId);

        return _targetTypeToDerivedType[targetType].Single(_ => _.Identifier == derivedTypeId).DerivedType;
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

    Type GetTargetTypeFrom(Type derivedType)
    {
        var attribute = derivedType.GetCustomAttribute<DerivedTypeAttribute>()!;
        var targetType = attribute.TargetType;
        var interfaces = derivedType.GetInterfaces().Where(_ => !_.Namespace?.StartsWith("System") ?? true).ToArray();

        if (targetType is null)
        {
            ThrowIfAmbiguousTargetTypeForDerivedType(derivedType, interfaces);
            ThrowIfMissingTargetTypeForDerivedType(derivedType, interfaces);
            targetType = interfaces[0];
        }

        ThrowIfMissingTargetTypeForDerivedType(derivedType, interfaces);
        ThrowIfTargetTypeMismatchesForDerivedType(derivedType, targetType, interfaces);

        return targetType;
    }

    void ThrowIfMissingDerivedTypeOrMissingIdentifier(Type targetType, DerivedTypeId derivedTypeId)
    {
        if (!_targetTypeToDerivedType.TryGetValue(targetType, out var value) ||
            !value.Any(_ => _.Identifier == derivedTypeId))
        {
            throw new MissingDerivedTypeForTargetType(targetType, derivedTypeId);
        }
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

    void ThrowIfTargetTypeMismatchesForDerivedType(Type derivedType, Type? targetType, Type[] interfaces)
    {
        if (!interfaces.Any(_ => _ == targetType))
        {
            throw new TargetTypeMismatchForDerivedType(derivedType);
        }
    }

    sealed record DerivedTypeAndIdentifier(Type DerivedType, Type TargetType, DerivedTypeId Identifier);
}
