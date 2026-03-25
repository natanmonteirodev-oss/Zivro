namespace Zivro.Domain.Entities;

/// <summary>
/// AuditLog entity for tracking user authentication activities.
/// Used for security auditing and detecting suspicious patterns.
/// </summary>
public class AuditLog : Entity
{
    /// <summary>
    /// User ID associated with this audit log (nullable for failed logins).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// User email for tracking (useful if user account is deleted).
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Type of action (Login, Logout, Register, FailedLogin, TwoFactorAttempt, etc.).
    /// </summary>
    public required string ActionType { get; set; }

    /// <summary>
    /// Whether the action was successful.
    /// </summary>
    public required bool IsSuccessful { get; set; }

    /// <summary>
    /// Reason for failure (if unsuccessful).
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// IP address of the request.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent from the request.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional metadata for the audit log.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Navigation property to the associated User.
    /// </summary>
    public virtual User? User { get; set; }
}
