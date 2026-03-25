namespace Zivro.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;
using Zivro.Infrastructure.Data;

/// <summary>
/// Repository for authentication-related database operations.
/// Implements database access for User and RefreshToken entities.
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly ZivroDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AuthRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AuthRepository(ZivroDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    public async Task CreateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _context.Users.AddAsync(user);
    }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to create.</param>
    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        if (refreshToken == null)
            throw new ArgumentNullException(nameof(refreshToken));

        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    /// <summary>
    /// Gets a refresh token by its value.
    /// </summary>
    /// <param name="token">The refresh token string.</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <summary>
    /// Gets all valid (non-revoked) refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>Collection of valid refresh tokens.</returns>
    public async Task<IEnumerable<RefreshToken>> GetValidRefreshTokensAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Enumerable.Empty<RefreshToken>();

        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="token">The refresh token to revoke.</param>
    public async Task RevokeRefreshTokenAsync(RefreshToken token)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        var trackedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == token.Id);

        if (trackedToken != null)
        {
            trackedToken.Revoke();
            _context.RefreshTokens.Update(trackedToken);
        }
    }

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
