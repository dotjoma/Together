namespace Together.Application.Interfaces;

/// <summary>
/// Service for logging security-sensitive operations for audit purposes
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs a security-sensitive operation
    /// </summary>
    Task LogAuditEventAsync(AuditEvent auditEvent);

    /// <summary>
    /// Logs user authentication events
    /// </summary>
    Task LogAuthenticationEventAsync(Guid? userId, string action, bool success, string? details = null);

    /// <summary>
    /// Logs data access events
    /// </summary>
    Task LogDataAccessEventAsync(Guid userId, string entityType, Guid entityId, string action);

    /// <summary>
    /// Logs privacy-related events
    /// </summary>
    Task LogPrivacyEventAsync(Guid userId, string action, string details);

    /// <summary>
    /// Logs security violations
    /// </summary>
    Task LogSecurityViolationAsync(Guid? userId, string violationType, string details);
}

/// <summary>
/// Represents an audit event
/// </summary>
public class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public bool Success { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
