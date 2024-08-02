// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.DependencyInjection;

/// <summary>
/// Attribute to adorn implementations that should be ignored by the IoC conventions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreConventionAttribute : Attribute;