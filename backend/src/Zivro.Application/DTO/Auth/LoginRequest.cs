namespace Zivro.Application.DTO.Auth;

/// <summary>
/// DTO for user login request.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's password (plain text).
    /// </summary>
    public required string Password { get; set; }
}
