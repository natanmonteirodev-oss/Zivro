namespace Zivro.Domain.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Repository interface for authentication-related database operations.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    Task CreateUserAsync(User user);

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to create.</param>
    Task CreateRefreshTokenAsync(RefreshToken refreshToken);

    /// <summary>
    /// Gets a refresh token by its value.
    /// </summary>
    /// <param name="token">The refresh token string.</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    Task<RefreshToken?> GetRefreshTokenAsync(string token);

    /// <summary>
    /// Gets all valid refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>Collection of valid (non-revoked) refresh tokens.</returns>
    Task<IEnumerable<RefreshToken>> GetValidRefreshTokensAsync(Guid userId);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="token">The refresh token to revoke.</param>
    Task RevokeRefreshTokenAsync(RefreshToken token);

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
