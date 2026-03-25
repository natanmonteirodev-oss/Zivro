namespace Zivro.Application.Services;

using System.Security.Cryptography;
using System.Text;
using Zivro.Application.Interfaces;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;

/// <summary>
/// Service for managing email verification.
/// </summary>
public class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IAuthRepository _authRepository;

    private const int VerificationTokenExpirationHours = 24;

    /// <summary>
    /// Initializes a new instance of EmailVerificationService.
    /// </summary>
    public EmailVerificationService(
        IEmailVerificationRepository emailVerificationRepository,
        IAuthRepository authRepository)
    {
        _emailVerificationRepository = emailVerificationRepository ?? throw new ArgumentNullException(nameof(emailVerificationRepository));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
    }

    /// <summary>
    /// Generates and sends an email verification link to the user.
    /// </summary>
    public async Task SendVerificationEmailAsync(Guid userId, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var token = GenerateVerificationToken();
        var expiresAt = DateTime.UtcNow.AddHours(VerificationTokenExpirationHours);

        var verification = new EmailVerification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email.ToLowerInvariant().Trim(),
            VerificationToken = token,
            ExpiresAt = expiresAt,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _emailVerificationRepository.CreateAsync(verification);
        await _emailVerificationRepository.SaveChangesAsync();

        // Generate verification link
        var verificationLink = $"https://api.zivro.com/api/auth/verify-email?userId={userId}&token={token}";

        // Send verification email (mock implementation)
        await SendVerificationLinkAsync(email, verificationLink);
    }

    /// <summary>
    /// Verifies an email using a verification token.
    /// </summary>
    public async Task<bool> VerifyEmailAsync(Guid userId, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        var verification = await _emailVerificationRepository.GetByTokenAsync(token);
        if (verification == null || verification.UserId != userId)
            return false;

        // Check if token has expired
        if (!verification.IsTokenValid())
            return false;

        // Check if already verified
        if (verification.IsVerified)
            return true;

        // Mark as verified
        verification.MarkAsVerified();
        await _emailVerificationRepository.UpdateAsync(verification);
        await _emailVerificationRepository.SaveChangesAsync();

        // Update user's email verification status
        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user != null)
        {
            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _authRepository.SaveChangesAsync();
        }

        return true;
    }

    /// <summary>
    /// Checks if an email verification is still valid.
    /// </summary>
    public async Task<bool> IsEmailVerifiedAsync(Guid userId)
    {
        var user = await _authRepository.GetUserByIdAsync(userId);
        return user?.EmailVerified ?? false;
    }

    /// <summary>
    /// Generates a secure verification token.
    /// </summary>
    public string GenerateVerificationToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to base64url-safe encoding
        var token = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return token;
    }

    /// <summary>
    /// Sends a verification email (mock implementation for development).
    /// </summary>
    public async Task SendVerificationLinkAsync(string email, string verificationLink)
    {
        // In production, this would integrate with email service (SendGrid, AWS SES, etc.)
        // For now, we'll just log it
        var emailContent = $@"
<html>
    <body>
        <h2>Bem-vindo à Zivro!</h2>
        <p>Por favor, clique no link abaixo para verificar seu email:</p>
        <p><a href='{verificationLink}'>Verificar Email</a></p>
        <p>Este link expira em 24 horas.</p>
        <p>Se você não se registrou, ignore este email.</p>
    </body>
</html>";

        // Log email sending (in development)
        Console.WriteLine($"[EMAIL] Verification email would be sent to: {email}");
        Console.WriteLine($"[EMAIL] Verification link: {verificationLink}");

        await Task.CompletedTask;
    }
}
