namespace Zivro.Application.Services;

using Zivro.Application.Interfaces;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;

/// <summary>
/// Service for managing audit logs of authentication activities.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Initializes a new instance of AuditLogService.
    /// </summary>
    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
    }

    /// <summary>
    /// Logs an authentication-related activity.
    /// </summary>
    public async Task<AuditLog> LogActionAsync(
        Guid? userId,
        string email,
        string actionType,
        bool isSuccessful,
        string? failureReason = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email.ToLowerInvariant().Trim(),
            ActionType = actionType,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        return await _auditLogRepository.CreateAsync(auditLog);
    }

    /// <summary>
    /// Gets number of failed login attempts for an email in a time period.
    /// </summary>
    public async Task<int> GetFailedLoginAttemptsAsync(string email, int minutesBack = 60)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutesBack);
        return await _auditLogRepository.GetFailedLoginAttemptsAsync(email.ToLowerInvariant(), since);
    }

    /// <summary>
    /// Gets number of failed 2FA attempts for a user in a time period.
    /// </summary>
    public async Task<int> GetFailed2FAAttemptsAsync(Guid userId, int minutesBack = 30)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutesBack);
        return await _auditLogRepository.GetFailed2FAAttemptsAsync(userId, since);
    }

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    public async Task<List<AuditLog>> GetUserAuditLogsAsync(Guid userId, int limit = 50)
    {
        return await _auditLogRepository.GetByUserIdAsync(userId, limit);
    }
}
