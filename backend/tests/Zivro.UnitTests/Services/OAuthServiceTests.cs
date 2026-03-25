namespace Zivro.UnitTests.Services;

using Moq;
using Xunit;
using FluentAssertions;
using Zivro.Application.Interfaces;
using Zivro.Application.Services;
using Zivro.Domain.Entities;
using Zivro.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

public class OAuthServiceTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ExternalLoginRepository> _externalLoginRepositoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<OAuthService>> _loggerMock;
    private readonly OAuthService _oauthService;

    public OAuthServiceTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _externalLoginRepositoryMock = new Mock<ExternalLoginRepository>(null!);
        _authRepositoryMock = new Mock<IAuthRepository>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<OAuthService>>();

        _oauthService = new OAuthService(
            _authServiceMock.Object,
            _externalLoginRepositoryMock.Object,
            _authRepositoryMock.Object,
            _serviceProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginWithOAuthAsync_NewUser_CreatesUserAndExternalLogin()
    {
        // Arrange
        var providerName = "Google";
        var code = "auth_code_123";
        var redirectUri = "https://localhost:3000/callback";
        var userEmail = "newuser@gmail.com";
        var userName = "New User";

        var providerMock = new Mock<IOAuthProvider>();
        var tokenResponse = new OAuthTokenResponse { AccessToken = "token123" };
        var userInfo = new OAuthUserInfo 
        { 
            Id = "google_123", 
            Email = userEmail,
            Name = userName,
            Picture = "https://picture.jpg"
        };

        providerMock.Setup(x => x.ExchangeCodeForTokenAsync(code, redirectUri))
            .ReturnsAsync(tokenResponse);
        providerMock.Setup(x => x.GetUserInfoAsync("token123"))
            .ReturnsAsync(userInfo);

        _serviceProviderMock
            .Setup(x => x.GetKeyedService<IOAuthProvider>(providerName))
            .Returns(providerMock.Object);

        _externalLoginRepositoryMock
            .Setup(x => x.GetByProviderUserIdAsync(providerName, "google_123"))
            .ReturnsAsync((ExternalLogin?)null);

        _authRepositoryMock
            .Setup(x => x.GetByEmailAsync(userEmail))
            .ReturnsAsync((User?)null);

        var jwtServiceMock = new Mock<IJwtTokenService>();
        jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), userEmail, userName))
            .Returns("jwt_token_123");

        _serviceProviderMock
            .Setup(x => x.GetRequiredService<IJwtTokenService>())
            .Returns(jwtServiceMock.Object);

        // Act
        var result = await _oauthService.LoginWithOAuthAsync(providerName, code, redirectUri);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt_token_123");
        result.TokenType.Should().Be("Bearer");

        _authRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _externalLoginRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ExternalLogin>()), Times.Once);
    }

    [Fact]
    public async Task LoginWithOAuthAsync_ExistingUser_UpdatesLastLogin()
    {
        // Arrange
        var providerName = "GitHub";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "user@github.com", Name = "User" };
        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = providerName,
            ProviderUserId = "github_456",
            User = user
        };

        var providerMock = new Mock<IOAuthProvider>();
        providerMock.Setup(x => x.ExchangeCodeForTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OAuthTokenResponse { AccessToken = "token456" });
        providerMock.Setup(x => x.GetUserInfoAsync("token456"))
            .ReturnsAsync(new OAuthUserInfo { Id = "github_456", Email = "user@github.com" });

        _serviceProviderMock
            .Setup(x => x.GetKeyedService<IOAuthProvider>(providerName))
            .Returns(providerMock.Object);

        _externalLoginRepositoryMock
            .Setup(x => x.GetByProviderUserIdAsync(providerName, "github_456"))
            .ReturnsAsync(externalLogin);

        var jwtServiceMock = new Mock<IJwtTokenService>();
        jwtServiceMock.Setup(x => x.GenerateAccessToken(userId, "user@github.com", "User"))
            .Returns("jwt_token_456");

        _serviceProviderMock
            .Setup(x => x.GetRequiredService<IJwtTokenService>())
            .Returns(jwtServiceMock.Object);

        // Act
        var result = await _oauthService.LoginWithOAuthAsync(providerName, "code", "redirectUri");

        // Assert
        result.UserId.Should().Be(userId);
        result.AccessToken.Should().Be("jwt_token_456");
        _externalLoginRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ExternalLogin>()), Times.Once);
    }

    [Fact]
    public async Task DisconnectExternalLoginAsync_ValidProvider_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerName = "Google";
        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = providerName
        };

        _externalLoginRepositoryMock
            .Setup(x => x.GetByProviderAsync(userId, providerName))
            .ReturnsAsync(externalLogin);

        // Act
        await _oauthService.DisconnectExternalLoginAsync(userId, providerName);

        // Assert
        _externalLoginRepositoryMock.Verify(
            x => x.DeleteAsync(It.Is<ExternalLogin>(el => el.Id == externalLogin.Id)),
            Times.Once);
    }
}
