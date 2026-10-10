// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Defines a value that can create a new instance of itself.
/// </summary>
/// <typeparam name="TSelf">The type that generates its own instances.</typeparam>
public interface IGeneratable<TSelf>
    where TSelf : IGeneratable<TSelf>
{
    /// <summary>
    /// Creates a new value using the type's generation strategy.
    /// </summary>
    /// <returns>A new instance of <typeparamref name="TSelf"/>.</returns>
    static abstract TSelf New();
}
