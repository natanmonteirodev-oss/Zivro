namespace Zivro.Application.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Interface for password reset repository operations
/// </summary>
public interface IPasswordResetRepository
{
    /// <summary>
    /// Get password reset request by token
    /// </summary>
    Task<PasswordReset?> GetByTokenAsync(string token);

    /// <summary>
    /// Get unused reset requests for a user
    /// </summary>
    Task<List<PasswordReset>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get count of recent reset requests by email (for rate limiting)
    /// </summary>
    Task<int> GetRecentRequestCountAsync(string email, int minutesBack = 60);

    /// <summary>
    /// Create new password reset request
    /// </summary>
    Task CreateAsync(PasswordReset passwordReset);

    /// <summary>
    /// Update password reset request
    /// </summary>
    Task UpdateAsync(PasswordReset passwordReset);
}
