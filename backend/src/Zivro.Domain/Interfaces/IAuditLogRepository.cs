namespace Zivro.Domain.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Repository interface for AuditLog data access operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    Task<AuditLog> CreateAsync(AuditLog auditLog);

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int limit = 50);

    /// <summary>
    /// Gets audit logs by action type.
    /// </summary>
    Task<List<AuditLog>> GetByActionTypeAsync(string actionType, int limit = 50);

    /// <summary>
    /// Gets failed login attempts for a specific email within a time range.
    /// </summary>
    Task<int> GetFailedLoginAttemptsAsync(string email, DateTime since);

    /// <summary>
    /// Gets failed 2FA attempts for a specific user within a time range.
    /// </summary>
    Task<int> GetFailed2FAAttemptsAsync(Guid userId, DateTime since);

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
