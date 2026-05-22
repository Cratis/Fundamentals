// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces;

/// <summary>
/// Represents a scope for an activity.
/// </summary>
/// <typeparam name="T">Type the scope is for.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ActivityScope{T}"/> class.
/// </remarks>
/// <param name="activity">The activity associated with the scope.</param>
public class ActivityScope<T>(System.Diagnostics.Activity? activity) : IActivityScope<T>
{
    /// <inheritdoc/>
    public System.Diagnostics.Activity? Activity { get; } = activity;

    /// <inheritdoc/>
    public void Dispose() => Activity?.Stop();
}
