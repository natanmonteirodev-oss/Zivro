namespace Zivro.Application.Interfaces;

using Zivro.Application.DTO;

/// <summary>
/// Serviço para gerenciar logins sociais (OAuth2)
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Obtém provedor OAuth pelo nome
    /// </summary>
    Task<IOAuthProvider> GetProviderAsync(string providerName);

    /// <summary>
    /// Realiza login/registro via OAuth
    /// </summary>
    Task<AuthResponse> LoginWithOAuthAsync(string providerName, string code, string redirectUri);

    /// <summary>
    /// Conecta um login externo a um usuário existente
    /// </summary>
    Task ConnectExternalLoginAsync(Guid userId, string providerName, string code, string redirectUri);

    /// <summary>
    /// Desconecta um login externo do usuário
    /// </summary>
    Task DisconnectExternalLoginAsync(Guid userId, string providerName);

    /// <summary>
    /// Lista logins externos do usuário
    /// </summary>
    Task<List<ExternalLoginDto>> GetExternalLoginsAsync(Guid userId);

    /// <summary>
    /// Verifica se um provedor está conectado
    /// </summary>
    Task<bool> HasExternalLoginAsync(Guid userId, string providerName);
}

/// <summary>
/// DTO para exibir informações de login externo
/// </summary>
public class ExternalLoginDto
{
    public string Provider { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
