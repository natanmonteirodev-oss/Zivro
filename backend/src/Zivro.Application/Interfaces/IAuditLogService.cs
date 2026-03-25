namespace Zivro.Application.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Service interface for audit logging operations.
/// Used to track authentication activities for security and compliance.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs an authentication-related activity.
    /// </summary>
    /// <param name="userId">The user ID (nullable for failed attempts).</param>
    /// <param name="email">The email or identifier being used.</param>
    /// <param name="actionType">Type of action (Login, Logout, Register, etc.).</param>
    /// <param name="isSuccessful">Whether the action succeeded.</param>
    /// <param name="failureReason">Reason for failure if unsuccessful.</param>
    /// <param name="ipAddress">IP address of the request.</param>
    /// <param name="userAgent">User agent string from the request.</param>
    /// <returns>The created AuditLog entity.</returns>
    Task<AuditLog> LogActionAsync(
        Guid? userId,
        string email,
        string actionType,
        bool isSuccessful,
        string? failureReason = null,
        string? ipAddress = null,
        string? userAgent = null
    );

    /// <summary>
    /// Gets recent failed login attempts for a specific email.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <param name="minutesBack">Number of minutes to look back (default: 60).</param>
    /// <returns>Number of failed login attempts in the time period.</returns>
    Task<int> GetFailedLoginAttemptsAsync(string email, int minutesBack = 60);

    /// <summary>
    /// Gets recent failed 2FA attempts for a specific user.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="minutesBack">Number of minutes to look back (default: 30).</param>
    /// <returns>Number of failed 2FA attempts in the time period.</returns>
    Task<int> GetFailed2FAAttemptsAsync(Guid userId, int minutesBack = 30);

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="limit">Maximum number of logs to return.</param>
    /// <returns>List of audit logs for the user.</returns>
    Task<List<AuditLog>> GetUserAuditLogsAsync(Guid userId, int limit = 50);
}
