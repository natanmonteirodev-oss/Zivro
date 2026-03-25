namespace Zivro.Infrastructure.Services.OAuth;

using System.Net.Http.Json;
using Zivro.Application.Interfaces;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Provedor OAuth para Google
/// </summary>
public class GoogleOAuthProvider : IOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string TokenUrl = "https://oauth2.googleapis.com/token";
    private const string UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

    public string ProviderName => "Google";

    public GoogleOAuthProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var clientId = _configuration["OAuth:Google:ClientId"];
        var clientSecret = _configuration["OAuth:Google:ClientSecret"];

        var request = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        };

        var content = new FormUrlEncodedContent(request);
        var response = await _httpClient.PostAsync(TokenUrl, content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();

        return new OAuthTokenResponse
        {
            AccessToken = tokenResponse.access_token,
            RefreshToken = tokenResponse.refresh_token,
            ExpiresIn = tokenResponse.expires_in,
            TokenType = tokenResponse.token_type
        };
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(UserInfoUrl);
        response.EnsureSuccessStatusCode();

        var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfoResponse>();

        return new OAuthUserInfo
        {
            Id = userInfo.id,
            Email = userInfo.email,
            Name = userInfo.name,
            Picture = userInfo.picture
        };
    }

    private class GoogleTokenResponse
    {
        public string access_token { get; set; } = default!;
        public string? refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; } = "Bearer";
    }

    private class GoogleUserInfoResponse
    {
        public string id { get; set; } = default!;
        public string email { get; set; } = default!;
        public string? name { get; set; }
        public string? picture { get; set; }
    }
}
