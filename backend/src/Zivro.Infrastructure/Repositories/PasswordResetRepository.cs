namespace Zivro.Infrastructure.Repositories;

using Zivro.Domain.Entities;
using Zivro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositório para gerenciar tokens de reset de senha
/// </summary>
public class PasswordResetRepository
{
    private readonly ZivroDbContext _context;

    public PasswordResetRepository(ZivroDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordReset?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResets
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<List<PasswordReset>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PasswordResets
            .Where(x => x.UserId == userId && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetRecentRequestCountAsync(string email, int minutesBack = 60)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutesBack);
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        
        if (user == null)
            return 0;

        return await _context.PasswordResets
            .Where(x => x.UserId == user.Id && x.CreatedAt >= since)
            .CountAsync();
    }

    public async Task CreateAsync(PasswordReset passwordReset)
    {
        _context.PasswordResets.Add(passwordReset);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PasswordReset passwordReset)
    {
        _context.PasswordResets.Update(passwordReset);
        await _context.SaveChangesAsync();
    }
}
