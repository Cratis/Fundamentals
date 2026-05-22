// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces;

/// <summary>
/// Defines a scope for an activity.
/// </summary>
/// <typeparam name="T">Type the scope is for.</typeparam>
public interface IActivityScope<T> : IDisposable
{
    /// <summary>
    /// Gets the <see cref="System.Diagnostics.Activity"/> associated with the scope.
    /// </summary>
    System.Diagnostics.Activity? Activity { get; }
}
