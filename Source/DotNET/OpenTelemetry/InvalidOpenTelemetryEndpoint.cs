// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry;

/// <summary>
/// The exception that is thrown when an OTLP endpoint is not an absolute HTTP or HTTPS URI.
/// </summary>
/// <param name="key">The invalid configuration key.</param>
public class InvalidOpenTelemetryEndpoint(string key) : Exception($"{key} must be an absolute HTTP or HTTPS URI.");
