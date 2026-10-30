// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry;

/// <summary>
/// The exception that is thrown when a metric policy contains an invalid setting.
/// </summary>
/// <param name="setting">The name of the invalid setting.</param>
/// <param name="reason">The reason the setting is invalid.</param>
public class InvalidMetricPolicy(string setting, string reason) : Exception($"{setting} {reason}");
