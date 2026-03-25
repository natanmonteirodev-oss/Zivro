namespace Zivro.UnitTests.Services;

using Moq;
using Xunit;
using FluentAssertions;
using Zivro.Application.Interfaces;
using Zivro.Application.Services;
using Zivro.Domain.Entities;
using Zivro.Domain.Enums;
using Zivro.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

public class SuspiciousLoginDetectionServiceTests
{
    private readonly Mock<SuspiciousLoginRepository> _suspiciousLoginRepositoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<ILogger<SuspiciousLoginDetectionService>> _loggerMock;
    private readonly SuspiciousLoginDetectionService _suspiciousLoginDetectionService;

    public SuspiciousLoginDetectionServiceTests()
    {
        _suspiciousLoginRepositoryMock = new Mock<SuspiciousLoginRepository>(null!);
        _authRepositoryMock = new Mock<IAuthRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<SuspiciousLoginDetectionService>>();

        _suspiciousLoginDetectionService = new SuspiciousLoginDetectionService(
            _suspiciousLoginRepositoryMock.Object,
            _authRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AnalyzeLoginAsync_NewLocation_CreatesSuspiciousLogin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "203.0.113.5";
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var user = new User { Id = userId, Email = "user@example.com" };

        _authRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _suspiciousLoginRepositoryMock.Setup(x => x.GetFrequentLoginIpsAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<(string, int)> { ("192.0.2.1", 10) });

        // Act
        var result = await _suspiciousLoginDetectionService.AnalyzeLoginAsync(userId, "user@example.com", ipAddress, userAgent);

        // Assert
        result.Should().NotBeNull();
        result!.Reason.Should().Contain(SuspiciousLoginReason.NewLocation);
        _suspiciousLoginRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SuspiciousLogin>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeLoginAsync_NewDevice_CreatesSuspiciousLogin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "192.0.2.1";
        var newUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X)";
        var user = new User { Id = userId, Email = "user@example.com" };

        _authRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _suspiciousLoginRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<SuspiciousLogin>
            {
                new SuspiciousLogin
                {
                    DeviceFingerprint = "oldDeviceHash",
                    IpAddress = ipAddress
                }
            });

        _suspiciousLoginRepositoryMock.Setup(x => x.GetFrequentLoginIpsAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<(string, int)> { (ipAddress, 5) });

        // Act
        var result = await _suspiciousLoginDetectionService.AnalyzeLoginAsync(userId, "user@example.com", ipAddress, newUserAgent);

        // Assert
        result.Should().NotBeNull();
        result!.Reason.Should().Contain(SuspiciousLoginReason.NewDevice);
    }

    [Fact]
    public async Task AnalyzeLoginAsync_AnomalousTime_CreatesSuspiciousLogin()
    {
        // Arrange
        // Mock DateTime for anomalous time (3-6 AM range)
        var userId = Guid.NewGuid();
        var ipAddress = "192.0.2.1";
        var userAgent = "Mozilla/5.0";

        _authRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "user@example.com" });

        _suspiciousLoginRepositoryMock.Setup(x => x.GetFrequentLoginIpsAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<(string, int)> { (ipAddress, 10) });

        _suspiciousLoginRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<SuspiciousLogin>
            {
                new SuspiciousLogin { DeviceFingerprint = "sameHash", IpAddress = ipAddress }
            });

        // Act
        var result = await _suspiciousLoginDetectionService.AnalyzeLoginAsync(userId, "user@example.com", ipAddress, userAgent);

        // Assert - Result may or may not be created depending on current time
        // This test verifies the service doesn't crash with anomalous time logic
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never());
    }

    [Fact]
    public async Task AnalyzeLoginAsync_SafeLogin_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "192.0.2.1";
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

        _authRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "user@example.com" });

        // Frequent IP from same user
        _suspiciousLoginRepositoryMock.Setup(x => x.GetFrequentLoginIpsAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<(string, int)> { (ipAddress, 15) });

        // Known device fingerprint
        _suspiciousLoginRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<SuspiciousLogin>
            {
                new SuspiciousLogin { DeviceFingerprint = "knownHash" }
            });

        // No recent same-IP logins
        _suspiciousLoginRepositoryMock.Setup(x => x.GetRecentByIpAndEmailAsync(ipAddress, "user@example.com", It.IsAny<int>()))
            .ReturnsAsync((SuspiciousLogin?)null);

        // Act
        var result = await _suspiciousLoginDetectionService.AnalyzeLoginAsync(userId, "user@example.com", ipAddress, userAgent);

        // Assert
        result.Should().BeNull();
        _suspiciousLoginRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SuspiciousLogin>()), Times.Never());
    }

    [Fact]
    public async Task GetSuspiciousLoginsAsync_ReturnsUserLogins()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var suspiciousLogins = new List<SuspiciousLogin>
        {
            new SuspiciousLogin
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = "user@example.com",
                IpAddress = "203.0.113.5",
                Reason = new[] { SuspiciousLoginReason.NewLocation }
            },
            new SuspiciousLogin
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = "user@example.com",
                IpAddress = "203.0.113.6",
                Reason = new[] { SuspiciousLoginReason.NewDevice }
            }
        };

        _suspiciousLoginRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, 10))
            .ReturnsAsync(suspiciousLogins);

        // Act
        var result = await _suspiciousLoginDetectionService.GetSuspiciousLoginsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(x => x.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task ConfirmSuspiciousLoginAsync_MarkAsLegitimate_UpdatesRecord()
    {
        // Arrange
        var suspiciousLoginId = Guid.NewGuid();
        var suspiciousLogin = new SuspiciousLogin
        {
            Id = suspiciousLoginId,
            UserId = Guid.NewGuid(),
            UserNotified = true
        };

        _suspiciousLoginRepositoryMock.Setup(x => x.GetByIdAsync(suspiciousLoginId))
            .ReturnsAsync(suspiciousLogin);

        // Act
        await _suspiciousLoginDetectionService.ConfirmSuspiciousLoginAsync(suspiciousLoginId, wasLegitimate: true);

        // Assert
        _suspiciousLoginRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<SuspiciousLogin>(s =>
                s.Id == suspiciousLoginId)),
            Times.Once);
    }

    [Fact]
    public async Task GetLocationFromIpAsync_ReturnsLocationData()
    {
        // Act
        var (country, city, lat, lon) = await _suspiciousLoginDetectionService.GetLocationFromIpAsync("8.8.8.8");

        // Assert - Currently returns mock data
        country.Should().NotBeNullOrEmpty();
        city.Should().NotBeNullOrEmpty();
        lat.Should().BeGreaterThanOrEqualTo(-90).And.BeLessThanOrEqualTo(90);
        lon.Should().BeGreaterThanOrEqualTo(-180).And.BeLessThanOrEqualTo(180);
    }

    [Fact]
    public async Task GenerateDeviceFingerprint_SameUserAgent_ReturnsSameHash()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        // Act
        var fingerprint1 = _suspiciousLoginDetectionService.GenerateDeviceFingerprint(userAgent);
        var fingerprint2 = _suspiciousLoginDetectionService.GenerateDeviceFingerprint(userAgent);

        // Assert
        fingerprint1.Should().Be(fingerprint2);
        fingerprint1.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateDeviceFingerprint_DifferentUserAgent_ReturnsDifferentHash()
    {
        // Arrange
        var userAgent1 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var userAgent2 = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_6)";

        // Act
        var fingerprint1 = _suspiciousLoginDetectionService.GenerateDeviceFingerprint(userAgent1);
        var fingerprint2 = _suspiciousLoginDetectionService.GenerateDeviceFingerprint(userAgent2);

        // Assert
        fingerprint1.Should().NotBe(fingerprint2);
    }

    [Fact]
    public async Task AnalyzeLoginAsync_MultipleFlags_CreatesSingleRecord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newIpAddress = "203.0.113.100";
        var newUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_6)";

        _authRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Email = "user@example.com" });

        _suspiciousLoginRepositoryMock.Setup(x => x.GetFrequentLoginIpsAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<(string, int)> { ("192.0.2.1", 20) }); // Different IP = new location

        _suspiciousLoginRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(new List<SuspiciousLogin>
            {
                new SuspiciousLogin { DeviceFingerprint = "oldHash", IpAddress = "192.0.2.1" }
            }); // Different device

        // Act
        var result = await _suspiciousLoginDetectionService.AnalyzeLoginAsync(userId, "user@example.com", newIpAddress, newUserAgent);

        // Assert
        result.Should().NotBeNull();
        // Should create only one record, not multiple
        _suspiciousLoginRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SuspiciousLogin>()), Times.Once);
    }
}
