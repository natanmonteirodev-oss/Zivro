namespace Zivro.Application.DTO;

/// <summary>
/// Response DTO for authentication operations
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token for authenticated requests
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for renewing access token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (typically "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// When the refresh token expires
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Authenticated user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User full name
    /// </summary>
    public string? Name { get; set; }
}
