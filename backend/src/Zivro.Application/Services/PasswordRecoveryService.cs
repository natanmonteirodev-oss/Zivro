namespace Zivro.Application.Services;

using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Zivro.Application.Interfaces;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;

/// <summary>
/// Serviço de recuperação de senha
/// </summary>
public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<PasswordRecoveryService> _logger;

    public PasswordRecoveryService(
        IPasswordResetRepository passwordResetRepository,
        IAuthRepository authRepository,
        IEmailVerificationService emailVerificationService,
        IPasswordHasher passwordHasher,
        ILogger<PasswordRecoveryService> logger)
    {
        _passwordResetRepository = passwordResetRepository;
        _authRepository = authRepository;
        _emailVerificationService = emailVerificationService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            // Não revela se o email existe (por segurança)
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
            return;
        }

        // Verifica rate limiting
        var recentRequests = await _passwordResetRepository.GetRecentRequestCountAsync(email);
        if (recentRequests >= 3)
        {
            _logger.LogWarning("Rate limit exceeded for password reset email: {Email}", email);
            throw new InvalidOperationException("Too many password reset requests. Please try again later.");
        }

        // Gera token seguro
        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var passwordReset = new PasswordReset
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _passwordResetRepository.CreateAsync(passwordReset);

        // Envia email com link de reset
        var resetLink = $"https://zivro.com/reset-password?token={token}";
        await SendPasswordResetEmailAsync(user.Email, user.Name, resetLink);

        _logger.LogInformation("Password reset requested for user {UserId}", user.Id);
    }

    public async Task<bool> ValidateResetTokenAsync(string token)
    {
        var passwordReset = await _passwordResetRepository.GetByTokenAsync(token);
        return passwordReset != null && passwordReset.IsValid;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var passwordReset = await _passwordResetRepository.GetByTokenAsync(token);
        
        if (passwordReset == null || !passwordReset.IsValid)
        {
            _logger.LogWarning("Invalid password reset token: {Token}", token);
            return false;
        }

        // Valida força da senha
        ValidatePassword(newPassword);

        // Atualiza senha do usuário
        var user = passwordReset.User;
        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _authRepository.SaveChangesAsync();

        // Marca token como usado
        passwordReset.UsedAt = DateTime.UtcNow;
        await _passwordResetRepository.UpdateAsync(passwordReset);

        _logger.LogInformation("Password reset successful for user {UserId}", user.Id);
        return true;
    }

    public async Task<int> GetResetRequestCountAsync(string email, int minutesBack = 60)
    {
        return await _passwordResetRepository.GetRecentRequestCountAsync(email, minutesBack);
    }

    private string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters");

        if (!password.Any(char.IsUpper))
            throw new InvalidOperationException("Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            throw new InvalidOperationException("Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            throw new InvalidOperationException("Password must contain at least one digit");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            throw new InvalidOperationException("Password must contain at least one special character");
    }

    private async Task SendPasswordResetEmailAsync(string email, string name, string resetLink)
    {
        var subject = "Reset Your Zivro Password";
        var htmlContent = $@"
            <h2>Password Reset Request</h2>
            <p>Hi {name},</p>
            <p>You requested to reset your password. Click the link below to set a new password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>This link will expire in 1 hour.</p>
            <p>If you didn't request this, please ignore this email.</p>
        ";

        // TODO: Implementar envio de email real
        await Task.Delay(10);
        System.Diagnostics.Debug.WriteLine($"[DEV] Password reset email sent to {email}: {resetLink}");
    }
}
