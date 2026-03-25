namespace Zivro.Application.DTO.Auth;

/// <summary>
/// DTO for authentication response containing tokens.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Unique identifier of the authenticated user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// JWT access token for API authorization.
    /// Short-lived (typically 15-30 minutes).
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// Long-lived (typically 7-30 days).
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// When the access token expires.
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// When the refresh token expires.
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// The type of token (typically "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
