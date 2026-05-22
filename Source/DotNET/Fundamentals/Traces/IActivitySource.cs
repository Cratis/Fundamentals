// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces;

/// <summary>
/// Defines an activity source for a specific type.
/// </summary>
/// <typeparam name="T">Type the activity source is for.</typeparam>
public interface IActivitySource<T>
{
    /// <summary>
    /// Gets the actual <see cref="System.Diagnostics.ActivitySource"/> instance.
    /// </summary>
    System.Diagnostics.ActivitySource ActualSource { get; }
}
