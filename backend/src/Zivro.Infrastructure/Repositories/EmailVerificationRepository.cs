namespace Zivro.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;
using Zivro.Infrastructure.Data;

/// <summary>
/// Repository for EmailVerification data access operations.
/// </summary>
public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly ZivroDbContext _context;

    /// <summary>
    /// Initializes a new instance of EmailVerificationRepository.
    /// </summary>
    public EmailVerificationRepository(ZivroDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Creates a new email verification record.
    /// </summary>
    public async Task<EmailVerification> CreateAsync(EmailVerification verification)
    {
        if (verification == null)
            throw new ArgumentNullException(nameof(verification));

        var entry = await _context.EmailVerifications.AddAsync(verification);
        return entry.Entity;
    }

    /// <summary>
    /// Gets an email verification by token.
    /// </summary>
    public async Task<EmailVerification?> GetByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        return await _context.EmailVerifications
            .FirstOrDefaultAsync(e => e.VerificationToken == token && e.IsActive);
    }

    /// <summary>
    /// Gets the latest email verification for a user.
    /// </summary>
    public async Task<EmailVerification?> GetLatestByUserIdAsync(Guid userId)
    {
        return await _context.EmailVerifications
            .Where(e => e.UserId == userId && e.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Updates an email verification record.
    /// </summary>
    public async Task<EmailVerification> UpdateAsync(EmailVerification verification)
    {
        if (verification == null)
            throw new ArgumentNullException(nameof(verification));

        _context.EmailVerifications.Update(verification);
        return verification;
    }

    /// <summary>
    /// Deletes an email verification record.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var verification = await _context.EmailVerifications.FindAsync(id);
        if (verification != null)
        {
            _context.EmailVerifications.Remove(verification);
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
