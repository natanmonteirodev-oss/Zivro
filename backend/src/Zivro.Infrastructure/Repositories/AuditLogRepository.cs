namespace Zivro.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;
using Zivro.Infrastructure.Data;

/// <summary>
/// Repository for AuditLog data access operations.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly ZivroDbContext _context;

    /// <summary>
    /// Initializes a new instance of AuditLogRepository.
    /// </summary>
    public AuditLogRepository(ZivroDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        if (auditLog == null)
            throw new ArgumentNullException(nameof(auditLog));

        var entry = await _context.AuditLogs.AddAsync(auditLog);
        return entry.Entity;
    }

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    public async Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int limit = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Gets audit logs by action type.
    /// </summary>
    public async Task<List<AuditLog>> GetByActionTypeAsync(string actionType, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("Action type cannot be empty", nameof(actionType));

        return await _context.AuditLogs
            .Where(a => a.ActionType == actionType && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Gets failed login attempts for a specific email within a time range.
    /// </summary>
    public async Task<int> GetFailedLoginAttemptsAsync(string email, DateTime since)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return await _context.AuditLogs
            .CountAsync(a =>
                a.Email == email.ToLowerInvariant() &&
                a.ActionType == "FailedLogin" &&
                !a.IsSuccessful &&
                a.CreatedAt >= since &&
                a.IsActive
            );
    }

    /// <summary>
    /// Gets failed 2FA attempts for a specific user within a time range.
    /// </summary>
    public async Task<int> GetFailed2FAAttemptsAsync(Guid userId, DateTime since)
    {
        return await _context.AuditLogs
            .CountAsync(a =>
                a.UserId == userId &&
                a.ActionType == "TwoFactorAttempt" &&
                !a.IsSuccessful &&
                a.CreatedAt >= since &&
                a.IsActive
            );
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
