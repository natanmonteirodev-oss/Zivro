namespace Zivro.Infrastructure.Services.OAuth;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Zivro.Application.Interfaces;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Provedor OAuth para GitHub
/// </summary>
public class GitHubOAuthProvider : IOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string TokenUrl = "https://github.com/login/oauth/access_token";
    private const string UserInfoUrl = "https://api.github.com/user";

    public string ProviderName => "GitHub";

    public GitHubOAuthProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];

        var request = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri }
        };

        var content = new FormUrlEncodedContent(request);
        var response = await _httpClient.PostAsync(TokenUrl, content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>();

        return new OAuthTokenResponse
        {
            AccessToken = tokenResponse.access_token,
            RefreshToken = null, // GitHub não retorna refresh token com public scope
            ExpiresIn = 0,
            TokenType = tokenResponse.token_type
        };
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Zivro-App");
        
        var response = await _httpClient.GetAsync(UserInfoUrl);
        response.EnsureSuccessStatusCode();

        var userInfo = await response.Content.ReadFromJsonAsync<GitHubUserInfoResponse>();

        return new OAuthUserInfo
        {
            Id = userInfo.id.ToString(),
            Email = userInfo.email ?? userInfo.login,
            Name = userInfo.name ?? userInfo.login,
            Picture = userInfo.avatar_url
        };
    }

    private class GitHubTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string access_token { get; set; } = default!;

        [JsonPropertyName("token_type")]
        public string token_type { get; set; } = "Bearer";

        [JsonPropertyName("scope")]
        public string? scope { get; set; }
    }

    private class GitHubUserInfoResponse
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("login")]
        public string login { get; set; } = default!;

        [JsonPropertyName("email")]
        public string? email { get; set; }

        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? avatar_url { get; set; }
    }
}
