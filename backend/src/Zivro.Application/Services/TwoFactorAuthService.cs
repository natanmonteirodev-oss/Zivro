namespace Zivro.Application.Services;

using System.Security.Cryptography;
using System.Text;
using Zivro.Application.Interfaces;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;

/// <summary>
/// Service for managing Two-Factor Authentication (2FA).
/// Supports TOTP (Time-based One-Time Password) via authenticator apps.
/// </summary>
public class TwoFactorAuthService : ITwoFactorAuthService
{
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
    private readonly IAuthRepository _authRepository;

    private const int MaxFailedAttempts = 5;
    private const int BackupCodesCount = 10;

    /// <summary>
    /// Initializes a new instance of TwoFactorAuthService.
    /// </summary>
    public TwoFactorAuthService(
        ITwoFactorAuthRepository twoFactorAuthRepository,
        IAuthRepository authRepository)
    {
        _twoFactorAuthRepository = twoFactorAuthRepository ?? throw new ArgumentNullException(nameof(twoFactorAuthRepository));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
    }

    /// <summary>
    /// Initiates 2FA setup for a user.
    /// </summary>
    public async Task<(string SecretKey, string QrCode, List<string> BackupCodes)> SetupTwoFactorAsync(Guid userId, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        // Generate secret key for TOTP
        var secretKey = GenerateSecretKey();
        var backupCodes = GenerateBackupCodes(BackupCodesCount);

        // Check if 2FA already exists for this user
        var existing2FA = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        if (existing2FA != null)
        {
            // Update existing configuration (not yet confirmed)
            existing2FA.SecretKey = secretKey;
            existing2FA.BackupCodes = string.Join(",", backupCodes);
            await _twoFactorAuthRepository.UpdateAsync(existing2FA);
        }
        else
        {
            // Create new 2FA configuration
            var twoFactorAuth = new TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SecretKey = secretKey,
                BackupCodes = string.Join(",", backupCodes),
                IsEnabled = false,
                FailedAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _twoFactorAuthRepository.CreateAsync(twoFactorAuth);
        }

        await _twoFactorAuthRepository.SaveChangesAsync();

        // Generate QR code data
        var qrCode = GenerateQrCode(email, secretKey);

        return (secretKey, qrCode, backupCodes);
    }

    /// <summary>
    /// Confirms 2FA setup by verifying the TOTP code.
    /// </summary>
    public async Task<bool> ConfirmTwoFactorAsync(Guid userId, string totpCode)
    {
        if (string.IsNullOrWhiteSpace(totpCode))
            throw new ArgumentException("TOTP code cannot be empty", nameof(totpCode));

        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        if (twoFactorAuth == null)
            throw new InvalidOperationException("2FA must be set up first");

        // Validate TOTP code
        if (!ValidateTotpCode(twoFactorAuth.SecretKey, totpCode))
        {
            twoFactorAuth.FailedAttempts++;
            if (twoFactorAuth.FailedAttempts >= MaxFailedAttempts)
            {
                // Lock the 2FA setup after too many failed attempts
                twoFactorAuth.IsActive = false;
            }
            await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
            await _twoFactorAuthRepository.SaveChangesAsync();
            return false;
        }

        // Enable 2FA
        twoFactorAuth.Enable();
        twoFactorAuth.FailedAttempts = 0;
        await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
        await _twoFactorAuthRepository.SaveChangesAsync();

        return true;
    }

    /// <summary>
    ///Disables 2FA for a user.
    /// </summary>
    public async Task DisableTwoFactorAsync(Guid userId)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        if (twoFactorAuth != null)
        {
            twoFactorAuth.Disable();
            await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
            await _twoFactorAuthRepository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Validates a TOTP code for a user.
    /// </summary>
    public async Task<bool> ValidateTotpCodeAsync(Guid userId, string totpCode)
    {
        if (string.IsNullOrWhiteSpace(totpCode))
            return false;

        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
            return false;

        if (twoFactorAuth.FailedAttempts >= MaxFailedAttempts)
            return false; // Account locked due to too many failed attempts

        // Validate TOTP code with time window (current ± 1 time step for clock skew)
        if (!ValidateTotpCode(twoFactorAuth.SecretKey, totpCode))
        {
            twoFactorAuth.RecordFailedAttempt();
            await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
            await _twoFactorAuthRepository.SaveChangesAsync();
            return false;
        }

        // Reset failed attempts on successful authentication
        twoFactorAuth.ResetFailedAttempts();
        await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
        await _twoFactorAuthRepository.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Validates a backup code for recovery.
    /// </summary>
    public async Task<bool> ValidateBackupCodeAsync(Guid userId, string backupCode)
    {
        if (string.IsNullOrWhiteSpace(backupCode))
            return false;

        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
            return false;

        if (string.IsNullOrEmpty(twoFactorAuth.BackupCodes))
            return false;

        var codes = twoFactorAuth.BackupCodes.Split(',');
        if (!codes.Contains(backupCode.Trim()))
            return false;

        // Remove used backup code
        var updatedCodes = string.Join(",", codes.Where(c => c != backupCode.Trim()));
        twoFactorAuth.BackupCodes = updatedCodes;
        twoFactorAuth.UpdatedAt = DateTime.UtcNow;

        await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
        await _twoFactorAuthRepository.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if 2FA is enabled for a user.
    /// </summary>
    public async Task<bool> IsTwoFactorEnabledAsync(Guid userId)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        return twoFactorAuth?.IsEnabled ?? false;
    }

    /// <summary>
    /// Regenerates backup codes.
    /// </summary>
    public async Task<List<string>> RegenerateBackupCodesAsync(Guid userId)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        if (twoFactorAuth == null)
            throw new InvalidOperationException("2FA is not configured for this user");

        var newBackupCodes = GenerateBackupCodes(BackupCodesCount);
        twoFactorAuth.BackupCodes = string.Join(",", newBackupCodes);
        twoFactorAuth.UpdatedAt = DateTime.UtcNow;

        await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
        await _twoFactorAuthRepository.SaveChangesAsync();

        return newBackupCodes;
    }

    // ============== Private Helper Methods ==============

    /// <summary>
    /// Generates a random secret key for TOTP.
    /// </summary>
    private static string GenerateSecretKey()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Base32 encoding for TOTP
        return ConvertToBase32(randomBytes);
    }

    /// <summary>
    /// Validates a TOTP code against the secret key.
    /// Allows for time skew of ±30 seconds (1 time window).
    /// </summary>
    private static bool ValidateTotpCode(string secretKey, string code)
    {
        if (!int.TryParse(code, out _))
            return false;

        try
        {
            // This is a simplified TOTP validation
            // In production, you should use OtpNet NuGet package for proper TOTP handling
            // For now, this is a placeholder that would need proper implementation
            
            // Convert secret key from Base32
            var secretBytes = ConvertFromBase32(secretKey);

            // Calculate TOTP for current time window
            var timeCounter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

            // Check current and adjacent time windows for clock skew
            for (int i = -1; i <= 1; i++)
            {
                var hmac = new System.Security.Cryptography.HMACSHA1(secretBytes);
                var counterBytes = BitConverter.GetBytes(timeCounter + i);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(counterBytes);

                var hmacResult = hmac.ComputeHash(counterBytes);
                var offset = hmacResult[hmacResult.Length - 1] & 0x0f;
                var totp = (
                    ((hmacResult[offset] & 0x7f) << 24) |
                    ((hmacResult[offset + 1] & 0xff) << 16) |
                    ((hmacResult[offset + 2] & 0xff) << 8) |
                    (hmacResult[offset + 3] & 0xff)
                ) % 1000000;

                if (totp.ToString("D6") == code)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates backup recovery codes.
    /// </summary>
    private static List<string> GenerateBackupCodes(int count)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var code = GenerateRandomCode(8);
            codes.Add(code);
        }

        return codes;
    }

    /// <summary>
    /// Generates a random alphanumeric code.
    /// </summary>
    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }

    /// <summary>
    /// Generates QR code data URL for authenticator app setup.
    /// </summary>
    private static string GenerateQrCode(string email, string secretKey)
    {
        // Format: otpauth://totp/Zivro:email?secret=secretkey&issuer=Zivro
        var escapedEmail = Uri.EscapeDataString(email);
        var otpauthUrl = $"otpauth://totp/Zivro:{escapedEmail}?secret={secretKey}&issuer=Zivro";

        // In production, generate actual QR code image using QRCoder or similar library
        // For now, return the otpauth URL
        return otpauthUrl;
    }

    /// <summary>
    /// Converts bytes to Base32 encoding.
    /// </summary>
    private static string ConvertToBase32(byte[] input)
    {
        if (input == null || input.Length == 0)
            throw new ArgumentException("Input cannot be empty");

        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new StringBuilder();
        int bitBuffer = 0;
        int bitCount = 0;

        foreach (byte b in input)
        {
            bitBuffer = (bitBuffer << 8) | b;
            bitCount += 8;

            while (bitCount >= 5)
            {
                bitCount -= 5;
                output.Append(alphabet[(bitBuffer >> bitCount) & 0x1F]);
            }
        }

        if (bitCount > 0)
        {
            output.Append(alphabet[(bitBuffer << (5 - bitCount)) & 0x1F]);
        }

        return output.ToString();
    }

    /// <summary>
    /// Converts Base32 encoded string to bytes.
    /// </summary>
    private static byte[] ConvertFromBase32(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>();
        int bitBuffer = 0;
        int bitCount = 0;

        foreach (char c in input.ToUpper())
        {
            int charIndex = alphabet.IndexOf(c);
            if (charIndex < 0)
                throw new ArgumentException($"Invalid Base32 character: {c}");

            bitBuffer = (bitBuffer << 5) | charIndex;
            bitCount += 5;

            if (bitCount >= 8)
            {
                bitCount -= 8;
                output.Add((byte)((bitBuffer >> bitCount) & 0xFF));
            }
        }

        return output.ToArray();
    }
}
