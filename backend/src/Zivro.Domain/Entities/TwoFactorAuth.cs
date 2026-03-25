namespace Zivro.Domain.Entities;

/// <summary>
/// TwoFactorAuth entity for storing user's 2FA configuration.
/// Supports TOTP (Time-based One-Time Password) via authenticator apps.
/// </summary>
public class TwoFactorAuth : Entity
{
    /// <summary>
    /// User ID this 2FA configuration is for.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Whether 2FA is enabled for this user.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The shared secret key for TOTP generation (encrypted in production).
    /// </summary>
    public required string SecretKey { get; set; }

    /// <summary>
    /// Backup codes for account recovery (stored securely, comma-separated).
    /// </summary>
    public string? BackupCodes { get; set; }

    /// <summary>
    /// When 2FA was enabled.
    /// </summary>
    public DateTime? EnabledAt { get; set; }

    /// <summary>
    /// When 2FA was last used (for security tracking).
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Number of failed 2FA attempts (reset after successful auth).
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// Navigation property to the associated User.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Enable 2FA for this user.
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        EnabledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disable 2FA for this user.
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record a failed 2FA attempt.
    /// </summary>
    public void RecordFailedAttempt()
    {
        FailedAttempts++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reset failed attempts after successful authentication.
    /// </summary>
    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
