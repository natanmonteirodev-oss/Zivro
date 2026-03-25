namespace Zivro.Application.Interfaces;

/// <summary>
/// Interface para integração com provedores de OAuth2
/// </summary>
public interface IOAuthProvider
{
    /// <summary>
    /// Nome do provedor (Google, GitHub, etc)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gera a URL de login do provedor
    /// </summary>
    //string GenerateLoginUrl(string redirectUri, string state);

    /// <summary>
    /// Troca authorization code por tokens
    /// </summary>
    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri);

    /// <summary>
    /// Obtém informações do usuário do provedor
    /// </summary>
    Task<OAuthUserInfo> GetUserInfoAsync(string accessToken);
}

/// <summary>
/// Resposta de token OAuth
/// </summary>
public class OAuthTokenResponse
{
    public string AccessToken { get; set; } = default!;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

/// <summary>
/// Informações do usuário retornadas pelo provedor OAuth
/// </summary>
public class OAuthUserInfo
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
