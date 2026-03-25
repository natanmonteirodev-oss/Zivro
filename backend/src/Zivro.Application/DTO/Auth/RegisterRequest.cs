namespace Zivro.Application.DTO.Auth;

/// <summary>
/// DTO for user registration request.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// User's full name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// User's email address (must be unique).
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's password (plain text, will be hashed).
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Password confirmation (should match Password).
    /// </summary>
    public required string PasswordConfirmation { get; set; }
}
