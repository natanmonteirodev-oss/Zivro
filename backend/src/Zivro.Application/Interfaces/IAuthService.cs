namespace Zivro.Application.Interfaces;

using Zivro.Application.DTO.Auth;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>Authentication response with tokens if successful.</returns>
    /// <exception cref="InvalidOperationException">If email already exists or validation fails.</exception>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user and returns tokens.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>Authentication response with tokens if successful.</returns>
    /// <exception cref="UnauthorizedAccessException">If credentials are invalid.</exception>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Refreshes an expired access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>New authentication response with fresh tokens.</returns>
    /// <exception cref="UnauthorizedAccessException">If refresh token is invalid or expired.</exception>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes a refresh token (logout).
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    Task RevokeTokenAsync(string refreshToken);

    /// <summary>
    /// Validates a user's credentials without issuing tokens.
    /// Used for additional verification steps.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>True if credentials are valid, false otherwise.</returns>
    Task<bool> ValidateUserAsync(string email, string password);
}
