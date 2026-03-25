namespace Zivro.Domain.Entities;

/// <summary>
/// User entity representing an authenticated application user.
/// </summary>
public class User : Entity
{
    /// <summary>
    /// User's full name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// User's email address (unique).
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Hashed password using SHA256 or BCrypt.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Whether the user's email has been verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Navigation property for user's refresh tokens.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Navigation property for user's audit logs.
    /// </summary>
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>
    /// Navigation property for user's email verification records.
    /// </summary>
    public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();

    /// <summary>
    /// Navigation property for user's 2FA configuration.
    /// </summary>
    public virtual TwoFactorAuth? TwoFactorAuth { get; set; }

    /// <summary>
    /// Checks if the user account is valid and active.
    /// </summary>
    public bool IsValidUser() => !string.IsNullOrWhiteSpace(Email) && 
                                !string.IsNullOrWhiteSpace(PasswordHash) && 
                                IsActive;
}
