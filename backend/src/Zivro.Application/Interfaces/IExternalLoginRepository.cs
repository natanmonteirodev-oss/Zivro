namespace Zivro.Application.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Interface for external login repository operations
/// </summary>
public interface IExternalLoginRepository
{
    /// <summary>
    /// Get external login by user ID and provider
    /// </summary>
    Task<ExternalLogin?> GetByProviderAsync(Guid userId, string provider);

    /// <summary>
    /// Get external login by provider user ID
    /// </summary>
    Task<ExternalLogin?> GetByProviderUserIdAsync(string provider, string providerUserId);

    /// <summary>
    /// Get all external logins for a user
    /// </summary>
    Task<List<ExternalLogin>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Create new external login
    /// </summary>
    Task CreateAsync(ExternalLogin externalLogin);

    /// <summary>
    /// Update existing external login
    /// </summary>
    Task UpdateAsync(ExternalLogin externalLogin);

    /// <summary>
    /// Delete external login
    /// </summary>
    Task DeleteAsync(ExternalLogin externalLogin);
}
