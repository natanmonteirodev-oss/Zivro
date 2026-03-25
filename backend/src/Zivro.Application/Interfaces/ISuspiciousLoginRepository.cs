namespace Zivro.Application.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Interface for suspicious login repository operations
/// </summary>
public interface ISuspiciousLoginRepository
{
    /// <summary>
    /// Get suspicious login by ID
    /// </summary>
    Task<SuspiciousLogin?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get recent suspicious logins for a user
    /// </summary>
    Task<List<SuspiciousLogin>> GetByUserIdAsync(Guid? userId, int limit = 10);

    /// <summary>
    /// Get recent suspicious login from same IP and email
    /// </summary>
    Task<SuspiciousLogin?> GetRecentByIpAndEmailAsync(string ipAddress, string email, int minutesBack = 5);

    /// <summary>
    /// Get frequently used IPs for a user
    /// </summary>
    Task<List<(string IpAddress, int Count)>> GetFrequentLoginIpsAsync(Guid userId, int limit = 5);

    /// <summary>
    /// Create new suspicious login record
    /// </summary>
    Task CreateAsync(SuspiciousLogin suspiciousLogin);

    /// <summary>
    /// Update suspicious login record
    /// </summary>
    Task UpdateAsync(SuspiciousLogin suspiciousLogin);
}
