namespace Zivro.Application.Interfaces;

using System.Security.Claims;

/// <summary>
/// Service interface for JWT token generation and validation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a new access token for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="name">The user's name.</param>
    /// <returns>A JWT access token string.</returns>
    string GenerateAccessToken(Guid userId, string email, string name);

    /// <summary>
    /// Validates and decodes a JWT token.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>ClaimsPrincipal if valid, null if invalid.</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extracts the user ID from a valid JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>User ID if found, null otherwise.</returns>
    Guid? ExtractUserId(string token);

    /// <summary>
    /// Extracts the user email from a valid JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>User email if found, null otherwise.</returns>
    string? ExtractEmail(string token);
}
