namespace Together.Application.Common;

/// <summary>
/// Provides correlation ID tracking for request tracing
/// </summary>
public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    /// <summary>
    /// Gets or sets the current correlation ID
    /// </summary>
    public static string CorrelationId
    {
        get => _correlationId.Value ?? GenerateCorrelationId();
        set => _correlationId.Value = value;
    }

    /// <summary>
    /// Generates a new correlation ID
    /// </summary>
    public static string GenerateCorrelationId()
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        _correlationId.Value = correlationId;
        return correlationId;
    }

    /// <summary>
    /// Clears the current correlation ID
    /// </summary>
    public static void Clear()
    {
        _correlationId.Value = null;
    }
}
