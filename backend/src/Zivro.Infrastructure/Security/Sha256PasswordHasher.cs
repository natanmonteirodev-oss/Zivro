namespace Zivro.Infrastructure.Security;

using System.Security.Cryptography;
using System.Text;
using Zivro.Domain.Interfaces;

/// <summary>
/// Password hasher using SHA256.
/// NOTE: This is a simple implementation for demonstration.
/// For production, use BCrypt, Argon2, or similar.
/// Can be easily replaced by changing the registration in DI.
/// </summary>
public class Sha256PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Number of iterations for key derivation (increases computational cost).
    /// </summary>
    private const int Iterations = 10000;

    /// <summary>
    /// Salt size in bytes.
    /// </summary>
    private const int SaltSize = 16;

    /// <summary>
    /// Hashes a password using PBKDF2-HMACSHA256.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>Base64 encoded salt + hash.</returns>
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // Generate random salt
        byte[] salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        // Derive key from password
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(32); // 256 bits

            // Combine salt and hash: salt + hash
            byte[] hashWithSalt = new byte[SaltSize + hash.Length];
            System.Buffer.BlockCopy(salt, 0, hashWithSalt, 0, SaltSize);
            System.Buffer.BlockCopy(hash, 0, hashWithSalt, SaltSize, hash.Length);

            return Convert.ToBase64String(hashWithSalt);
        }
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hash">The stored hash (salt + hash).</param>
    /// <returns>True if password matches the hash.</returns>
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // Decode the hash
            byte[] hashBytes = Convert.FromBase64String(hash);

            if (hashBytes.Length < SaltSize)
                return false;

            // Extract salt from hash
            byte[] salt = new byte[SaltSize];
            System.Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            // Derive key from input password using the same salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash2 = pbkdf2.GetBytes(32);

                // Compare the derived hash with stored hash
                for (int i = 0; i < hash2.Length; i++)
                {
                    if (hashBytes[i + SaltSize] != hash2[i])
                        return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
