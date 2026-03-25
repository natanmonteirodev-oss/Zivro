namespace Zivro.Application.Interfaces;

/// <summary>
/// Service interface for Two-Factor Authentication (2FA) operations.
/// Supports TOTP (Time-based One-Time Password) via authenticator apps.
/// </summary>
public interface ITwoFactorAuthService
{
    /// <summary>
    /// Initiates 2FA setup for a user, generating a secret key.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>Setup information with QR code and manual entry key.</returns>
    Task<(string SecretKey, string QrCode, List<string> BackupCodes)> SetupTwoFactorAsync(Guid userId, string email);

    /// <summary>
    /// Confirms 2FA setup by verifying the TOTP code.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="totpCode">The TOTP code from the authenticator app.</param>
    /// <returns>True if the code is valid and 2FA is enabled, false otherwise.</returns>
    Task<bool> ConfirmTwoFactorAsync(Guid userId, string totpCode);

    /// <summary>
    /// Disables 2FA for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>Task representing the async operation.</returns>
    Task DisableTwoFactorAsync(Guid userId);

    /// <summary>
    /// Validates a TOTP code for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="totpCode">The TOTP code to validate.</param>
    /// <returns>True if the code is valid, false otherwise.</returns>
    Task<bool> ValidateTotpCodeAsync(Guid userId, string totpCode);

    /// <summary>
    /// Validates a backup code for a user (used as recovery mechanism).
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="backupCode">The backup code to validate.</param>
    /// <returns>True if the backup code is valid, false otherwise.</returns>
    Task<bool> ValidateBackupCodeAsync(Guid userId, string backupCode);

    /// <summary>
    /// Checks if 2FA is enabled for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>True if 2FA is enabled, false otherwise.</returns>
    Task<bool> IsTwoFactorEnabledAsync(Guid userId);

    /// <summary>
    /// Generates new backup codes for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>List of new backup codes.</returns>
    Task<List<string>> RegenerateBackupCodesAsync(Guid userId);
}
