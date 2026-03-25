namespace Zivro.Domain.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Repository interface for TwoFactorAuth data access operations.
/// </summary>
public interface ITwoFactorAuthRepository
{
    /// <summary>
    /// Gets 2FA configuration for a user.
    /// </summary>
    Task<TwoFactorAuth?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Creates a new 2FA configuration for a user.
    /// </summary>
    Task<TwoFactorAuth> CreateAsync(TwoFactorAuth twoFactorAuth);

    /// <summary>
    /// Updates 2FA configuration.
    /// </summary>
    Task<TwoFactorAuth> UpdateAsync(TwoFactorAuth twoFactorAuth);

    /// <summary>
    /// Deletes 2FA configuration for a user.
    /// </summary>
    Task DeleteAsync(Guid userId);

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
