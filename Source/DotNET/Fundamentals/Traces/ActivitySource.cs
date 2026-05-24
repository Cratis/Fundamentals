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
/// <param name="activitySource">The actual activity source being used. When resolved as a keyed service, DI provides the keyed <see cref="DiagnosticsActivitySource"/>. When resolved as a non-keyed service and no source is provided, one named from <typeparamref name="T"/> is created.</param>
public class ActivitySource<T>([FromKeyedServices] DiagnosticsActivitySource? activitySource = null) : IActivitySource<T>
{
    /// <inheritdoc/>
    public DiagnosticsActivitySource ActualSource { get; } = activitySource ?? new(typeof(T).FullName ?? typeof(T).Name);
}
