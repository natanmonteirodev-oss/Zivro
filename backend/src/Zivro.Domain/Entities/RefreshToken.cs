namespace Zivro.Domain.Entities;

/// <summary>
/// RefreshToken entity for long-lived authentication tokens.
/// Used to issue new access tokens without requiring user credentials.
/// </summary>
public class RefreshToken : Entity
{
    /// <summary>
    /// Foreign key to the User.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// The actual refresh token value (JWT or random string).
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// When the refresh token expires.
    /// </summary>
    public required DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this refresh token has been revoked (invalidated).
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Navigation property to the associated User.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Checks if the token is still valid (not expired and not revoked).
    /// </summary>
    public bool IsValid() => !IsRevoked && ExpiresAt > DateTime.UtcNow && IsActive;

    /// <summary>
    /// Revokes this refresh token.
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
