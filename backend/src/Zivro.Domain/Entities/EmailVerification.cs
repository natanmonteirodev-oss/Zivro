namespace Zivro.Domain.Entities;

/// <summary>
/// EmailVerification entity for tracking email verification tokens and status.
/// </summary>
public class EmailVerification : Entity
{
    /// <summary>
    /// User ID this verification is for.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// The email address being verified.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Unique verification token (should be cryptographically secure).
    /// </summary>
    public required string VerificationToken { get; set; }

    /// <summary>
    /// When this verification token expires.
    /// </summary>
    public required DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the email was verified (null if not yet verified).
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Whether this verification has been completed.
    /// </summary>
    public bool IsVerified => VerifiedAt.HasValue;

    /// <summary>
    /// Number of verification attempts made.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Navigation property to the associated User.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Checks if the verification token is still valid.
    /// </summary>
    public bool IsTokenValid() => !IsVerified && ExpiresAt > DateTime.UtcNow && IsActive;

    /// <summary>
    /// Marks the email as verified.
    /// </summary>
    public void MarkAsVerified()
    {
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
