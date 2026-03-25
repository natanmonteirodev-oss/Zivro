namespace Zivro.IntegrationTests;

using Xunit;
using FluentAssertions;
using Zivro.Application.Interfaces;
using Zivro.Application.Services;
using Zivro.Domain.Entities;
using Zivro.Infrastructure.Repositories;
using Moq;
using Microsoft.Extensions.Logging;

public class AuthenticationFlowIntegrationTests
{
    [Fact]
    public async Task CompleteOAuthLoginFlow_NewUser_CreatesAccountAndReturnsToken()
    {
        // Arrange
        var googleProviderId = "google_12345";
        var email = "newuser@gmail.com";
        var displayName = "New User";
        var profilePictureUrl = "https://example.com/avatar.jpg";

        // Setup mocks
        var authRepositoryMock = new Mock<IAuthRepository>();
        var externalLoginRepositoryMock = new Mock<ExternalLoginRepository>(null!);
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<OAuthService>>();

        var googleOAuthProvider = new Mock<IOAuthProvider>();
        googleOAuthProvider
            .Setup(x => x.ExchangeCodeForTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OAuthTokenResponse 
            { 
                AccessToken = "google_token",
                ExpiresIn = 3600,
                TokenType = "Bearer"
            });

        googleOAuthProvider
            .Setup(x => x.GetUserInfoAsync(It.IsAny<string>()))
            .ReturnsAsync(new OAuthUserInfo
            {
                Id = googleProviderId,
                Email = email,
                Name = displayName,
                Picture = profilePictureUrl,
                FirstName = "New",
                LastName = "User"
            });

        serviceProviderMock
            .Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns(googleOAuthProvider.Object);

        // New user doesn't exist yet
        authRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Setup auth service to create user
        User createdUser = null!;
        authRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns(Task.CompletedTask);

        // JWT generation
        authRepositoryMock
            .Setup(x => x.GenerateJwtTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("jwt_token_here");

        var oAuthService = new OAuthService(
            authRepositoryMock.Object,
            externalLoginRepositoryMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object);

        // Act
        var result = await oAuthService.LoginWithOAuthAsync("Google", "auth_code", "https://localhost:7249/oauth/callback");

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt_token_here");
        result.TokenType.Should().Be("Bearer");
        
        // Verify user was created
        authRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        
        // Verify external login was created
        externalLoginRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<ExternalLogin>(el =>
                el.Provider == "Google" &&
                el.ProviderUserId == googleProviderId &&
                el.Email == email)),
            Times.Once);
    }

    [Fact]
    public async Task CompletePasswordRecoveryFlow_RequestAndReset_Success()
    {
        // Arrange
        var email = "user@example.com";
        var userId = Guid.NewGuid();
        var newPassword = "NewPassword@123";

        var passwordResetRepositoryMock = new Mock<PasswordResetRepository>(null!);
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailServiceMock = new Mock<IEmailVerificationService>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var loggerMock = new Mock<ILogger<PasswordRecoveryService>>();

        var user = new User { Id = userId, Email = email, PasswordHash = "old_hash" };

        // Setup mocks for request
        authRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        passwordResetRepositoryMock
            .Setup(x => x.GetRecentRequestCountAsync(email, It.IsAny<int>()))
            .ReturnsAsync(0);

        // Email service mock
        emailServiceMock
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        PasswordReset createdReset = null!;
        passwordResetRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<PasswordReset>()))
            .Callback<PasswordReset>(pr => createdReset = pr)
            .Returns(Task.CompletedTask);

        var passwordRecoveryService = new PasswordRecoveryService(
            passwordResetRepositoryMock.Object,
            authRepositoryMock.Object,
            emailServiceMock.Object,
            passwordHasherMock.Object,
            loggerMock.Object);

        // Act - Request password reset
        await passwordRecoveryService.RequestPasswordResetAsync(email);

        // Assert - Request succeeded
        createdReset.Should().NotBeNull();
        createdReset.UserId.Should().Be(userId);
        createdReset.Token.Should().NotBeNullOrEmpty();
        emailServiceMock.Verify(
            x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);

        // Act - Reset password with valid token
        passwordResetRepositoryMock
            .Setup(x => x.GetByTokenAsync(createdReset.Token))
            .ReturnsAsync(createdReset);

        passwordHasherMock
            .Setup(x => x.Hash(newPassword))
            .Returns("hashed_new_password");

        var resetResult = await passwordRecoveryService.ResetPasswordAsync(createdReset.Token, newPassword);

        // Assert - Reset succeeded
        resetResult.Should().BeTrue();
        user.PasswordHash.Should().Be("hashed_new_password");
        createdReset.UsedAt.Should().NotBeNull();
        authRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task SuspiciousLoginDetectionFlow_MultipleFlags_NotifiesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var newIpAddress = "203.0.113.100"; // Different from known IPs
        var newUserAgent = "Mozilla/5.0 (iPhone OS)"; // Different device

        var suspiciousLoginRepositoryMock = new Mock<SuspiciousLoginRepository>(null!);
        var authRepositoryMock = new Mock<IAuthRepository>();
        var auditLogServiceMock = new Mock<IAuditLogService>();
        var loggerMock = new Mock<ILogger<SuspiciousLoginDetectionService>>();

        var user = new User { Id = userId, Email = email };

        authRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Known IPs from this user
        suspiciousLoginRepositoryMock
            .Setup(x => x.GetFrequentLoginIpsAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<(string, int)> { ("192.0.2.1", 20), ("192.0.2.2", 10) });

        // Previous logins with different device
        suspiciousLoginRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<SuspiciousLogin>
            {
                new SuspiciousLogin 
                { 
                    DeviceFingerprint = "windows_desktop_hash",
                    IpAddress = "192.0.2.1"
                }
            });

        SuspiciousLogin createdSuspiciousLogin = null!;
        suspiciousLoginRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SuspiciousLogin>()))
            .Callback<SuspiciousLogin>(sl => createdSuspiciousLogin = sl)
            .Returns(Task.CompletedTask);

        var suspiciousLoginDetectionService = new SuspiciousLoginDetectionService(
            suspiciousLoginRepositoryMock.Object,
            authRepositoryMock.Object,
            auditLogServiceMock.Object,
            loggerMock.Object);

        // Act
        var result = await suspiciousLoginDetectionService.AnalyzeLoginAsync(userId, email, newIpAddress, newUserAgent);

        // Assert - Suspicious login detected
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.IpAddress.Should().Be(newIpAddress);
        result.Reason.Should().NotBeEmpty();
        
        // Verify record was created
        suspiciousLoginRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SuspiciousLogin>()), Times.Once);
    }

    [Fact]
    public async Task AccountLinkingFlow_ConnectExternalProvider_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var githubProviderId = "github_67890";

        var authRepositoryMock = new Mock<IAuthRepository>();
        var externalLoginRepositoryMock = new Mock<ExternalLoginRepository>(null!);
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<OAuthService>>();

        var user = new User { Id = userId, Email = email };

        authRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // No existing GitHub login
        externalLoginRepositoryMock
            .Setup(x => x.GetByProviderAsync(userId, "GitHub"))
            .ReturnsAsync((ExternalLogin?)null);

        var githubOAuthProvider = new Mock<IOAuthProvider>();
        githubOAuthProvider
            .Setup(x => x.ExchangeCodeForTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OAuthTokenResponse 
            { 
                AccessToken = "github_token",
                ExpiresIn = 3600,
                TokenType = "Bearer"
            });

        githubOAuthProvider
            .Setup(x => x.GetUserInfoAsync(It.IsAny<string>()))
            .ReturnsAsync(new OAuthUserInfo
            {
                Id = githubProviderId,
                Email = "user@github.com",
                Name = "User Name",
                Picture = "https://avatars.githubusercontent.com/u/67890"
            });

        serviceProviderMock
            .Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns(githubOAuthProvider.Object);

        var oAuthService = new OAuthService(
            authRepositoryMock.Object,
            externalLoginRepositoryMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object);

        // Act
        await oAuthService.ConnectExternalLoginAsync(userId, "GitHub", "github_auth_code", "https://localhost:7249/oauth/callback");

        // Assert
        externalLoginRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<ExternalLogin>(el =>
                el.UserId == userId &&
                el.Provider == "GitHub" &&
                el.ProviderUserId == githubProviderId)),
            Times.Once);
    }
}

// Helper DTOs for OAuth
public class OAuthTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

public class OAuthUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public Guid UserId { get; set; }
}
