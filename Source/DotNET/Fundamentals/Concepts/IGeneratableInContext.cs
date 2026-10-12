// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Defines a value that can create a new instance using the operation's time or tenant.
/// </summary>
/// <typeparam name="TSelf">The type that generates its own instances.</typeparam>
/// <remarks>
/// If a type also implements <see cref="IGeneratable{TSelf}"/>, callers use this context form.
/// </remarks>
public interface IGeneratableInContext<TSelf>
    where TSelf : IGeneratableInContext<TSelf>
{
    /// <summary>
    /// Creates a new value using the type's context-dependent generation strategy.
    /// </summary>
    /// <param name="context">The operation's captured time and portable tenant identifier.</param>
    /// <returns>A new instance of <typeparamref name="TSelf"/>.</returns>
    static abstract TSelf New(GenerationContext context);
}
