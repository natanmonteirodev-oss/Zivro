namespace Zivro.Infrastructure.Repositories;

using Zivro.Domain.Entities;
using Zivro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositório para gerenciar logins externos (OAuth)
/// </summary>
public class ExternalLoginRepository
{
    private readonly ZivroDbContext _context;

    public ExternalLoginRepository(ZivroDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalLogin?> GetByProviderAsync(Guid userId, string provider)
    {
        return await _context.ExternalLogins
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider);
    }

    public async Task<ExternalLogin?> GetByProviderUserIdAsync(string provider, string providerUserId)
    {
        return await _context.ExternalLogins
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId);
    }

    public async Task<List<ExternalLogin>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ExternalLogins
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastLoginAt)
            .ToListAsync();
    }

    public async Task CreateAsync(ExternalLogin externalLogin)
    {
        _context.ExternalLogins.Add(externalLogin);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ExternalLogin externalLogin)
    {
        _context.ExternalLogins.Update(externalLogin);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ExternalLogin externalLogin)
    {
        _context.ExternalLogins.Remove(externalLogin);
        await _context.SaveChangesAsync();
    }
}
