using System.IO;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Together.Services;

/// <summary>
/// Configures Serilog logging for the application
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog with file and console sinks
    /// </summary>
    public static ILogger ConfigureSerilog()
    {
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Together",
            "Logs");

        Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, "together-.log");

        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                fileSizeLimitBytes: 10_485_760, // 10 MB
                rollOnFileSizeLimit: true)
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(logDirectory, "together-structured-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();
    }

    /// <summary>
    /// Sanitizes sensitive data from log messages
    /// </summary>
    public static string SanitizeLogMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        // List of sensitive keywords to redact
        var sensitiveKeywords = new[]
        {
            "password",
            "token",
            "secret",
            "apikey",
            "api_key",
            "authorization",
            "bearer",
            "jwt",
            "refresh_token",
            "access_token"
        };

        var sanitized = message;
        foreach (var keyword in sensitiveKeywords)
        {
            // Case-insensitive replacement
            var pattern = new System.Text.RegularExpressions.Regex(
                $@"({keyword}[""']?\s*[:=]\s*[""']?)([^""'\s,}}]+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            sanitized = pattern.Replace(sanitized, "$1***REDACTED***");
        }

        return sanitized;
    }
}
