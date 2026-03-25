namespace Zivro.UnitTests.Services;

using Moq;
using Xunit;
using FluentAssertions;
using Zivro.Application.Interfaces;
using Zivro.Application.Services;
using Zivro.Domain.Entities;
using Zivro.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

public class PasswordRecoveryServiceTests
{
    private readonly Mock<PasswordResetRepository> _passwordResetRepositoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IEmailVerificationService> _emailServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<PasswordRecoveryService>> _loggerMock;
    private readonly PasswordRecoveryService _passwordRecoveryService;

    public PasswordRecoveryServiceTests()
    {
        _passwordResetRepositoryMock = new Mock<PasswordResetRepository>(null!);
        _authRepositoryMock = new Mock<IAuthRepository>();
        _emailServiceMock = new Mock<IEmailVerificationService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<PasswordRecoveryService>>();

        _passwordRecoveryService = new PasswordRecoveryService(
            _passwordResetRepositoryMock.Object,
            _authRepositoryMock.Object,
            _emailServiceMock.Object,
            _passwordHasherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_ValidEmail_CreatesToken()
    {
        // Arrange
        var email = "user@example.com";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = email, Name = "User" };

        _authRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _passwordResetRepositoryMock.Setup(x => x.GetRecentRequestCountAsync(email, It.IsAny<int>()))
            .ReturnsAsync(0);

        // Act
        await _passwordRecoveryService.RequestPasswordResetAsync(email);

        // Assert
        _passwordResetRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<PasswordReset>(pr =>
                pr.UserId == userId &&
                pr.Token != null &&
                pr.ExpiresAt > DateTime.UtcNow)),
            Times.Once);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_RateLimitExceeded_ThrowsException()
    {
        // Arrange
        var email = "user@example.com";
        var user = new User { Id = Guid.NewGuid(), Email = email };

        _authRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _passwordResetRepositoryMock.Setup(x => x.GetRecentRequestCountAsync(email, It.IsAny<int>()))
            .ReturnsAsync(3);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _passwordRecoveryService.RequestPasswordResetAsync(email));
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_UpdatesPassword()
    {
        // Arrange
        var token = "valid_token_123";
        var newPassword = "NewPassword@123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "user@example.com", PasswordHash = "old_hash" };
        var passwordReset = new PasswordReset
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = user
        };

        _passwordResetRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(passwordReset);

        _passwordHasherMock.Setup(x => x.Hash(newPassword))
            .Returns("new_hash");

        // Act
        var result = await _passwordRecoveryService.ResetPasswordAsync(token, newPassword);

        // Assert
        result.Should().BeTrue();
        _authRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _passwordResetRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PasswordReset>()), Times.Once);
        passwordReset.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var token = "expired_token";
        var passwordReset = new PasswordReset
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1)
        };

        _passwordResetRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(passwordReset);

        // Act
        var result = await _passwordRecoveryService.ResetPasswordAsync(token, "NewPassword@123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateResetTokenAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var token = "valid_token";
        var passwordReset = new PasswordReset
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };

        _passwordResetRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(passwordReset);

        // Act
        var result = await _passwordRecoveryService.ValidateResetTokenAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_WeakPassword_ThrowsException()
    {
        // Arrange
        var token = "token";
        var weakPassword = "weak"; // Muito curta
        var passwordReset = new PasswordReset
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new User()
        };

        _passwordResetRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(passwordReset);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _passwordRecoveryService.ResetPasswordAsync(token, weakPassword));
    }
}
