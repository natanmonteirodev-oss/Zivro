namespace Zivro.UnitTests.Services.OAuth;

using Moq;
using Xunit;
using FluentAssertions;
using System.Net;
using Zivro.Infrastructure.Services.OAuth;
using Microsoft.Extensions.Logging;

public class GoogleOAuthProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<GoogleOAuthProvider>> _loggerMock;
    private readonly GoogleOAuthProvider _googleOAuthProvider;

    public GoogleOAuthProviderTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<GoogleOAuthProvider>>();

        _googleOAuthProvider = new GoogleOAuthProvider(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_ValidCode_ReturnsOAuthTokenResponse()
    {
        // Arrange
        var code = "auth_code_123";
        var redirectUri = "https://localhost:7249/oauth/callback";
        var responseContent = new
        {
            access_token = "google_access_token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(responseContent))
            });

        // Act
        var result = await _googleOAuthProvider.ExchangeCodeForTokenAsync(code, redirectUri);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("google_access_token");
        result.ExpiresIn.Should().Be(3600);
        result.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_InvalidCode_ThrowsException()
    {
        // Arrange
        var code = "invalid_code";
        var redirectUri = "https://localhost:7249/oauth/callback";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\": \"invalid_code\"}")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _googleOAuthProvider.ExchangeCodeForTokenAsync(code, redirectUri));
    }

    [Fact]
    public async Task GetUserInfoAsync_ValidAccessToken_ReturnsOAuthUserInfo()
    {
        // Arrange
        var accessToken = "valid_access_token";
        var userInfoContent = new
        {
            id = "google_user_123",
            email = "user@gmail.com",
            name = "John Doe",
            picture = "https://example.com/photo.jpg",
            given_name = "John",
            family_name = "Doe"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(userInfoContent))
            });

        // Act
        var result = await _googleOAuthProvider.GetUserInfoAsync(accessToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("google_user_123");
        result.Email.Should().Be("user@gmail.com");
        result.Name.Should().Be("John Doe");
        result.Picture.Should().Be("https://example.com/photo.jpg");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetUserInfoAsync_InvalidToken_ThrowsException()
    {
        // Arrange
        var accessToken = "invalid_token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\": \"invalid_token\"}")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _googleOAuthProvider.GetUserInfoAsync(accessToken));
    }
}

public class GitHubOAuthProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<GitHubOAuthProvider>> _loggerMock;
    private readonly GitHubOAuthProvider _githubOAuthProvider;

    public GitHubOAuthProviderTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<GitHubOAuthProvider>>();

        _githubOAuthProvider = new GitHubOAuthProvider(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_ValidCode_ReturnsOAuthTokenResponse()
    {
        // Arrange
        var code = "github_code_456";
        var redirectUri = "https://localhost:7249/oauth/callback";
        var responseContent = "access_token=github_access_token&token_type=bearer&scope=user";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _githubOAuthProvider.ExchangeCodeForTokenAsync(code, redirectUri);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("github_access_token");
        result.TokenType.Should().Be("bearer");
    }

    [Fact]
    public async Task GetUserInfoAsync_ValidAccessToken_ReturnsOAuthUserInfo()
    {
        // Arrange
        var accessToken = "github_valid_token";
        var userInfoContent = new
        {
            id = 12345,
            login = "johndoe",
            email = "john@github.com",
            name = "John Doe",
            avatar_url = "https://avatars.githubusercontent.com/u/12345",
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(userInfoContent))
            });

        // Act
        var result = await _githubOAuthProvider.GetUserInfoAsync(accessToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("12345");
        result.Email.Should().Be("john@github.com");
        result.Name.Should().Be("John Doe");
        result.Picture.Should().Be("https://avatars.githubusercontent.com/u/12345");
    }

    [Fact]
    public async Task GetUserInfoAsync_NoEmail_UsesLoginAsEmail()
    {
        // Arrange
        var accessToken = "github_token";
        var userInfoContent = new
        {
            id = 12345,
            login = "johndoe",
            email = (string?)null,
            name = "John Doe",
            avatar_url = "https://avatars.githubusercontent.com/u/12345"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(userInfoContent))
            });

        // Act
        var result = await _githubOAuthProvider.GetUserInfoAsync(accessToken);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("johndoe"); // Login used as fallback
    }
}
