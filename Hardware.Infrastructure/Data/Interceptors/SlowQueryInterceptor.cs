using System.Data.Common;
using Hardware.Shared.Configuration;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hardware.Infrastructure.Data.Interceptors;

public sealed class SlowQueryInterceptor(
    ILogger<SlowQueryInterceptor> logger,
    IOptions<AppSettings> appSettings) : DbCommandInterceptor
{
    private readonly int _thresholdMs = appSettings.Value.SlowQueryThresholdMs;

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Log(eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        Log(eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, object? result,
        CancellationToken cancellationToken = default)
    {
        Log(eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void Log(CommandExecutedEventData eventData)
    {
        if (eventData.Duration.TotalMilliseconds < _thresholdMs) return;

        logger.LogWarning(
            "Slow SQL detected: {DurationMs} ms (threshold {ThresholdMs} ms): {CommandText}",
            eventData.Duration.TotalMilliseconds,
            _thresholdMs,
            eventData.Command.CommandText);
    }
}
