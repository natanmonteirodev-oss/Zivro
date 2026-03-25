namespace Zivro.Domain.Interfaces;

/// <summary>
/// Interface for password hashing and verification.
/// Allows for easy implementation replacement (e.g., SHA256, BCrypt, Argon2).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain text password against a hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The password hash to verify against.</param>
    /// <returns>True if the password matches the hash, false otherwise.</returns>
    bool Verify(string password, string hash);
}
