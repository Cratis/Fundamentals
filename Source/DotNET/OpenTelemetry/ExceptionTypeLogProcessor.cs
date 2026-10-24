// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Represents a log processor that replaces exception message and stack trace details with only the exception type.
/// </summary>
sealed class ExceptionTypeLogProcessor : BaseProcessor<LogRecord>
{
    /// <summary>
    /// Removes exception message and stack trace attributes and records the exception type name instead.
    /// </summary>
    /// <param name="data">The log record being processed.</param>
    public override void OnEnd(LogRecord data)
    {
        var attributes = data.Attributes?.Where(attribute => attribute.Key is not ("exception.message" or "exception.stacktrace")).ToList() ?? [];
        if (data.Exception is { } exception)
        {
            attributes.RemoveAll(attribute => attribute.Key == "exception.type");
            attributes.Add(new("exception.type", exception.GetType().FullName));
            data.Exception = null;
            data.FormattedMessage = null;
        }
        data.Attributes = attributes;
    }
}
