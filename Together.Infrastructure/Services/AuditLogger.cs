using Microsoft.Extensions.Logging;
using Together.Application.Interfaces;

namespace Together.Infrastructure.Services;

/// <summary>
/// Implementation of audit logging service for security-sensitive operations
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    public Task LogAuditEventAsync(AuditEvent auditEvent)
    {
        if (auditEvent == null)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "[AUDIT] EventId: {EventId}, Type: {EventType}, Action: {Action}, UserId: {UserId}, " +
            "EntityType: {EntityType}, EntityId: {EntityId}, Success: {Success}, Details: {Details}, " +
            "Timestamp: {Timestamp}",
            auditEvent.Id,
            auditEvent.EventType,
            auditEvent.Action,
            auditEvent.UserId,
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.Success,
            auditEvent.Details,
            auditEvent.Timestamp);

        return Task.CompletedTask;
    }

    public Task LogAuthenticationEventAsync(Guid? userId, string action, bool success, string? details = null)
    {
        var auditEvent = new AuditEvent
        {
            UserId = userId,
            EventType = "Authentication",
            Action = action,
            Success = success,
            Details = details
        };

        return LogAuditEventAsync(auditEvent);
    }

    public Task LogDataAccessEventAsync(Guid userId, string entityType, Guid entityId, string action)
    {
        var auditEvent = new AuditEvent
        {
            UserId = userId,
            EventType = "DataAccess",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Success = true
        };

        return LogAuditEventAsync(auditEvent);
    }

    public Task LogPrivacyEventAsync(Guid userId, string action, string details)
    {
        var auditEvent = new AuditEvent
        {
            UserId = userId,
            EventType = "Privacy",
            Action = action,
            Details = details,
            Success = true
        };

        return LogAuditEventAsync(auditEvent);
    }

    public Task LogSecurityViolationAsync(Guid? userId, string violationType, string details)
    {
        var auditEvent = new AuditEvent
        {
            UserId = userId,
            EventType = "SecurityViolation",
            Action = violationType,
            Details = details,
            Success = false
        };

        _logger.LogWarning(
            "[SECURITY VIOLATION] Type: {ViolationType}, UserId: {UserId}, Details: {Details}",
            violationType,
            userId,
            details);

        return LogAuditEventAsync(auditEvent);
    }
}
