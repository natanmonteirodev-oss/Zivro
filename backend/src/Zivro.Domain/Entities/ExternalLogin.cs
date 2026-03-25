namespace Zivro.Domain.Entities;

/// <summary>
/// Representa um login externo (OAuth2) do usuário
/// </summary>
public class ExternalLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = default!; // "Google", "GitHub"
    public string ProviderUserId { get; set; } = default!; // ID from external provider
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}
