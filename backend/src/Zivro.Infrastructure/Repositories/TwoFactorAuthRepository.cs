namespace Zivro.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;
using Zivro.Infrastructure.Data;

/// <summary>
/// Repository for TwoFactorAuth data access operations.
/// </summary>
public class TwoFactorAuthRepository : ITwoFactorAuthRepository
{
    private readonly ZivroDbContext _context;

    /// <summary>
    /// Initializes a new instance of TwoFactorAuthRepository.
    /// </summary>
    public TwoFactorAuthRepository(ZivroDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets 2FA configuration for a user.
    /// </summary>
    public async Task<TwoFactorAuth?> GetByUserIdAsync(Guid userId)
    {
        return await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId && t.IsActive);
    }

    /// <summary>
    /// Creates a new 2FA configuration for a user.
    /// </summary>
    public async Task<TwoFactorAuth> CreateAsync(TwoFactorAuth twoFactorAuth)
    {
        if (twoFactorAuth == null)
            throw new ArgumentNullException(nameof(twoFactorAuth));

        var entry = await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
        return entry.Entity;
    }

    /// <summary>
    /// Updates 2FA configuration.
    /// </summary>
    public async Task<TwoFactorAuth> UpdateAsync(TwoFactorAuth twoFactorAuth)
    {
        if (twoFactorAuth == null)
            throw new ArgumentNullException(nameof(twoFactorAuth));

        _context.TwoFactorAuths.Update(twoFactorAuth);
        return twoFactorAuth;
    }

    /// <summary>
    /// Deletes 2FA configuration for a user.
    /// </summary>
    public async Task DeleteAsync(Guid userId)
    {
        var twoFactorAuth = await GetByUserIdAsync(userId);
        if (twoFactorAuth != null)
        {
            _context.TwoFactorAuths.Remove(twoFactorAuth);
        }
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
