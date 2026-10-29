// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Represents a filter that excludes health and readiness requests from tracing.
/// </summary>
static class HealthRequestFilter
{
    static readonly string[] _paths = ["/health", "/healthz", "/ready", "/readyz", "/readiness"];

    /// <summary>
    /// Determines whether a request should be traced.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <returns><see langword="true"/> if the request path is not a health or readiness path; otherwise <see langword="false"/>.</returns>
    internal static bool ShouldTrace(HttpContext context) => !_paths.Any(path => context.Request.Path.StartsWithSegments(path));
}
