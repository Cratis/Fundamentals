// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using DiagnosticsActivitySource = System.Diagnostics.ActivitySource;

namespace Cratis.Traces;

/// <summary>
/// Represents a typed <see cref="DiagnosticsActivitySource"/>.
/// </summary>
/// <typeparam name="T">Type the activity source is for.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ActivitySource{T}"/> class.
/// </remarks>
/// <param name="activitySource">The actual activity source being used.</param>
/// <param name="serviceProvider">The service provider.</param>
/// <param name="serviceKey">The current service key, if any.</param>
public class ActivitySource<T>(DiagnosticsActivitySource? activitySource = null, IServiceProvider? serviceProvider = null, [ServiceKey] object? serviceKey = null) : IActivitySource<T>
{
    /// <inheritdoc/>
    public DiagnosticsActivitySource ActualSource { get; } =
        activitySource ??
        (serviceKey is not null && serviceProvider is not null ? serviceProvider.GetKeyedService<DiagnosticsActivitySource>(serviceKey) : null) ??
        new(typeof(T).FullName ?? typeof(T).Name);
}
