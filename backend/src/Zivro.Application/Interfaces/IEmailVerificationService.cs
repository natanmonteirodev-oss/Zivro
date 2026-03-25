namespace Zivro.Application.Interfaces;

/// <summary>
/// Service interface for email verification operations.
/// Handles sending verification emails and validating email addresses.
/// </summary>
public interface IEmailVerificationService
{
    /// <summary>
    /// Generates and sends an email verification link to the user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="email">The email address to verify.</param>
    /// <returns>Task representing the async operation.</returns>
    Task SendVerificationEmailAsync(Guid userId, string email);

    /// <summary>
    /// Verifies an email using a verification token.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="token">The verification token.</param>
    /// <returns>True if verification succeeded, false otherwise.</returns>
    Task<bool> VerifyEmailAsync(Guid userId, string token);

    /// <summary>
    /// Checks if an email verification is still valid (not expired).
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>True if email is verified, false otherwise.</returns>
    Task<bool> IsEmailVerifiedAsync(Guid userId);

    /// <summary>
    /// Generates a secure verification token.
    /// </summary>
    /// <returns>A cryptographically secure token.</returns>
    string GenerateVerificationToken();

    /// <summary>
    /// Sends a verification email (mock implementation for development).
    /// </summary>
    /// <param name="email">The recipient email.</param>
    /// <param name="verificationLink">The verification link.</param>
    /// <returns>Task representing the async operation.</returns>
    Task SendVerificationLinkAsync(string email, string verificationLink);
}
