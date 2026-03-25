namespace Zivro.Application.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zivro.Application.DTO;
using Zivro.Application.Interfaces;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;

/// <summary>
/// Serviço de autenticação OAuth2
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly IAuthService _authService;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IAuthService authService,
        IExternalLoginRepository externalLoginRepository,
        IAuthRepository authRepository,
        IServiceProvider serviceProvider,
        ILogger<OAuthService> logger)
    {
        _authService = authService;
        _externalLoginRepository = externalLoginRepository;
        _authRepository = authRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<IOAuthProvider> GetProviderAsync(string providerName)
    {
        var provider = _serviceProvider.GetKeyedService<IOAuthProvider>(providerName);
        if (provider == null)
        {
            throw new InvalidOperationException($"OAuth provider '{providerName}' not found");
        }
        return provider;
    }

    public async Task<AuthResponse> LoginWithOAuthAsync(string providerName, string code, string redirectUri)
    {
        var provider = await GetProviderAsync(providerName);
        var tokenResponse = await provider.ExchangeCodeForTokenAsync(code, redirectUri);
        var userInfo = await provider.GetUserInfoAsync(tokenResponse.AccessToken);

        // Procura login externo existente
        var externalLogin = await _externalLoginRepository.GetByProviderUserIdAsync(providerName, userInfo.Id);

        User user;
        if (externalLogin != null)
        {
            // Usuário já existe, atualiza último login
            user = externalLogin.User;
            externalLogin.LastLoginAt = DateTime.UtcNow;
            await _externalLoginRepository.UpdateAsync(externalLogin);

            _logger.LogInformation("User {UserId} logged in via {Provider}", user.Id, providerName);
        }
        else
        {
            // Usuário novo ou email já existe no sistema
            user = await _authRepository.GetUserByEmailAsync(userInfo.Email);

            if (user == null)
            {
                // Cria novo usuário
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = userInfo.Email,
                    Name = userInfo.Name ?? userInfo.Email.Split('@')[0],
                    PasswordHash = "OAUTH_LOGIN", // Não possui senha, usa OAuth
                    IsActive = true,
                    EmailVerified = true // OAuth emails são considerados verificados
                };

                await _authRepository.CreateUserAsync(user);
                _logger.LogInformation("New user {UserId} created via {Provider}", user.Id, providerName);
            }

            // Cria login externo
            var newExternalLogin = new ExternalLogin
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = providerName,
                ProviderUserId = userInfo.Id,
                Email = userInfo.Email,
                DisplayName = userInfo.Name,
                ProfilePictureUrl = userInfo.Picture,
                ConnectedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _externalLoginRepository.CreateAsync(newExternalLogin);
            _logger.LogInformation("External login connected for user {UserId} provider {Provider}", user.Id, providerName);
        }

        // Gera novo access token JWT
        var jwtTokenService = _serviceProvider.GetRequiredService<IJwtTokenService>();
        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.Name);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            AccessToken = accessToken,
            RefreshToken = "", // Implementar refresh token logic se necessário
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            TokenType = "Bearer"
        };
    }

    public async Task ConnectExternalLoginAsync(Guid userId, string providerName, string code, string redirectUri)
    {
        var provider = await GetProviderAsync(providerName);
        var tokenResponse = await provider.ExchangeCodeForTokenAsync(code, redirectUri);
        var userInfo = await provider.GetUserInfoAsync(tokenResponse.AccessToken);

        // Verifica se já existe
        var existing = await _externalLoginRepository.GetByProviderAsync(userId, providerName);
        if (existing != null)
        {
            throw new InvalidOperationException($"This {providerName} account is already connected");
        }

        // Verifica se otro usuário tem este provedor
        var conflicting = await _externalLoginRepository.GetByProviderUserIdAsync(providerName, userInfo.Id);
        if (conflicting != null)
        {
            throw new InvalidOperationException($"This {providerName} account is already connected to another user");
        }

        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = providerName,
            ProviderUserId = userInfo.Id,
            Email = userInfo.Email,
            DisplayName = userInfo.Name,
            ProfilePictureUrl = userInfo.Picture,
            ConnectedAt = DateTime.UtcNow
        };

        await _externalLoginRepository.CreateAsync(externalLogin);
        _logger.LogInformation("External login {Provider} connected for user {UserId}", providerName, userId);
    }

    public async Task DisconnectExternalLoginAsync(Guid userId, string providerName)
    {
        var externalLogin = await _externalLoginRepository.GetByProviderAsync(userId, providerName);
        if (externalLogin == null)
        {
            throw new InvalidOperationException($"External login {providerName} not found");
        }

        await _externalLoginRepository.DeleteAsync(externalLogin);
        _logger.LogInformation("External login {Provider} disconnected for user {UserId}", providerName, userId);
    }

    public async Task<List<ExternalLoginDto>> GetExternalLoginsAsync(Guid userId)
    {
        var logins = await _externalLoginRepository.GetByUserIdAsync(userId);
        return logins.Select(x => new ExternalLoginDto
        {
            Provider = x.Provider,
            Email = x.Email,
            DisplayName = x.DisplayName,
            ProfilePictureUrl = x.ProfilePictureUrl,
            ConnectedAt = x.ConnectedAt,
            LastLoginAt = x.LastLoginAt
        }).ToList();
    }

    public async Task<bool> HasExternalLoginAsync(Guid userId, string providerName)
    {
        var login = await _externalLoginRepository.GetByProviderAsync(userId, providerName);
        return login != null;
    }
}
