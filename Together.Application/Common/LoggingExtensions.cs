using Microsoft.Extensions.Logging;

namespace Together.Application.Common;

/// <summary>
/// Extension methods for structured logging with correlation IDs
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs information with correlation ID
    /// </summary>
    public static void LogInformationWithCorrelation(this ILogger logger, string message, params object[] args)
    {
        var correlationId = CorrelationContext.CorrelationId;
        var argsWithCorrelation = new object[] { correlationId }.Concat(args).ToArray();
        logger.LogInformation($"[CorrelationId: {{CorrelationId}}] {message}", argsWithCorrelation);
    }

    /// <summary>
    /// Logs warning with correlation ID
    /// </summary>
    public static void LogWarningWithCorrelation(this ILogger logger, string message, params object[] args)
    {
        var correlationId = CorrelationContext.CorrelationId;
        var argsWithCorrelation = new object[] { correlationId }.Concat(args).ToArray();
        logger.LogWarning($"[CorrelationId: {{CorrelationId}}] {message}", argsWithCorrelation);
    }

    /// <summary>
    /// Logs error with correlation ID
    /// </summary>
    public static void LogErrorWithCorrelation(this ILogger logger, Exception exception, string message, params object[] args)
    {
        var correlationId = CorrelationContext.CorrelationId;
        var argsWithCorrelation = new object[] { correlationId }.Concat(args).ToArray();
        logger.LogError(exception, $"[CorrelationId: {{CorrelationId}}] {message}", argsWithCorrelation);
    }

    /// <summary>
    /// Logs debug with correlation ID
    /// </summary>
    public static void LogDebugWithCorrelation(this ILogger logger, string message, params object[] args)
    {
        var correlationId = CorrelationContext.CorrelationId;
        var argsWithCorrelation = new object[] { correlationId }.Concat(args).ToArray();
        logger.LogDebug($"[CorrelationId: {{CorrelationId}}] {message}", argsWithCorrelation);
    }

    /// <summary>
    /// Begins a logging scope with correlation ID
    /// </summary>
    public static IDisposable? BeginCorrelationScope(this ILogger logger, string operationName)
    {
        var correlationId = CorrelationContext.GenerateCorrelationId();
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Operation"] = operationName
        });
    }
}
