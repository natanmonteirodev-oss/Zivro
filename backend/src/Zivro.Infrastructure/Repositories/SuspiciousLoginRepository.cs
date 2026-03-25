namespace Zivro.Infrastructure.Repositories;

using Zivro.Domain.Entities;
using Zivro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositório para gerenciar logins suspeitos
/// </summary>
public class SuspiciousLoginRepository
{
    private readonly ZivroDbContext _context;

    public SuspiciousLoginRepository(ZivroDbContext context)
    {
        _context = context;
    }

    public async Task<SuspiciousLogin?> GetByIdAsync(Guid id)
    {
        return await _context.SuspiciousLogins
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<SuspiciousLogin>> GetByUserIdAsync(Guid? userId, int limit = 10)
    {
        return await _context.SuspiciousLogins
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DetectedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<SuspiciousLogin?> GetRecentByIpAndEmailAsync(string ipAddress, string email, int minutesBack = 5)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutesBack);
        return await _context.SuspiciousLogins
            .Where(x => x.IpAddress == ipAddress && x.Email == email && x.DetectedAt >= since)
            .OrderByDescending(x => x.DetectedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<(string IpAddress, int Count)>> GetFrequentLoginIpsAsync(Guid userId, int limit = 5)
    {
        return await _context.SuspiciousLogins
            .Where(x => x.UserId == userId && !x.UserNotified)
            .GroupBy(x => x.IpAddress)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => new ValueTuple<string, int>(g.Key, g.Count()))
            .ToListAsync();
    }

    public async Task CreateAsync(SuspiciousLogin suspiciousLogin)
    {
        _context.SuspiciousLogins.Add(suspiciousLogin);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SuspiciousLogin suspiciousLogin)
    {
        _context.SuspiciousLogins.Update(suspiciousLogin);
        await _context.SaveChangesAsync();
    }
}
