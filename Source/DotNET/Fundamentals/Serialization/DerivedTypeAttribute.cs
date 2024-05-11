// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Cratis.Serialization;

/// <summary>
/// Attribute used to adorn types that represent a unique type in the system and serializers need to recognize.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DerivedTypeAttribute"/> class.
/// </remarks>
/// <param name="identifier">String representation of a <see cref="Guid"/>.</param>
/// <param name="targetType">Optional target type the derived type is for.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DerivedTypeAttribute(string identifier, Type? targetType = default) : Attribute
{
    /// <summary>
    /// Gets the unique identifier of the derived type.
    /// </summary>
    public DerivedTypeId Identifier { get; } = identifier;

    /// <summary>
    /// Gets the optional target type.
    /// </summary>
    public Type? TargetType { get; } = targetType;
}
