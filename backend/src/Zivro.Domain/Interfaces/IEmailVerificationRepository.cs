namespace Zivro.Domain.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Repository interface for EmailVerification data access operations.
/// </summary>
public interface IEmailVerificationRepository
{
    /// <summary>
    /// Creates a new email verification record.
    /// </summary>
    Task<EmailVerification> CreateAsync(EmailVerification verification);

    /// <summary>
    /// Gets an email verification by token.
    /// </summary>
    Task<EmailVerification?> GetByTokenAsync(string token);

    /// <summary>
    /// Gets the latest email verification for a user.
    /// </summary>
    Task<EmailVerification?> GetLatestByUserIdAsync(Guid userId);

    /// <summary>
    /// Updates an email verification record.
    /// </summary>
    Task<EmailVerification> UpdateAsync(EmailVerification verification);

    /// <summary>
    /// Deletes an email verification record.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
