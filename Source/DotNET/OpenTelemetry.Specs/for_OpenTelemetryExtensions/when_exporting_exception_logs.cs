// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_exporting_exception_logs : given.a_configured_environment
{
    readonly log_exporter _exporter = new();

    void Because() => Export(_exporter, false);

    static void Export(log_exporter exporter, bool includeDetails)
    {
        var builder = CreateBuilder();
        builder.AddCratisOpenTelemetry(options => options.IncludeExceptionDetails = includeDetails);
        builder.Services.ConfigureOpenTelemetryLoggerProvider((_, logging) => logging.AddProcessor(new SimpleLogRecordExportProcessor(exporter)));
        using var services = builder.Services.BuildServiceProvider();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Cratis.Logging.Specs");
        try
        {
            throw new InvalidOperationException("private exception value");
        }
        catch (InvalidOperationException exception)
        {
            LoggerMessage.Define(LogLevel.Error, new EventId(1), "Operation failed")(logger, exception);
        }
    }

    [Fact] void should_export_a_log_record() => _exporter.Count.ShouldEqual(1);
    [Fact]
    void should_allow_exception_details_only_with_explicit_opt_in()
    {
        using var exporter = new log_exporter();
        Export(exporter, true);
        exporter.Exception!.Message.ShouldEqual("private exception value");
        exporter.Exception.StackTrace.ShouldNotBeNull();
    }
    [Fact] void should_keep_the_exception_type() => _exporter.Attributes.ShouldContain(new KeyValuePair<string, object?>("exception.type", typeof(InvalidOperationException).FullName));
    [Fact] void should_not_export_the_exception_object() => _exporter.Exception.ShouldBeNull();
    [Fact] void should_not_export_formatted_exception_messages() => _exporter.FormattedMessage.ShouldBeNull();
    [Fact] void should_not_export_exception_messages_or_stacks() => _exporter.Attributes.Exists(attribute => string.Equals(attribute.Key, "exception.message", StringComparison.Ordinal) || string.Equals(attribute.Key, "exception.stacktrace", StringComparison.Ordinal)).ShouldBeFalse();

    sealed class log_exporter : BaseExporter<LogRecord>
    {
        public int Count;
        public Exception? Exception;
        public string? FormattedMessage;
        public List<KeyValuePair<string, object?>> Attributes = [];

        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            foreach (var record in batch)
            {
                Count++;
                Exception = record.Exception;
                FormattedMessage = record.FormattedMessage;
                Attributes = [.. record.Attributes ?? []];
            }
            return ExportResult.Success;
        }
    }
}
