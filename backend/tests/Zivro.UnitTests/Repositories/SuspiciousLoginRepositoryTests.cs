namespace Zivro.UnitTests.Repositories;

using Moq;
using Xunit;
using FluentAssertions;
using Zivro.Domain.Entities;
using Zivro.Domain.Enums;
using Zivro.Infrastructure.Repositories;
using Zivro.Infrastructure;

public class SuspiciousLoginRepositoryTests
{
    private readonly Mock<ZivroDbContext> _dbContextMock;
    private readonly SuspiciousLoginRepository _repository;

    public SuspiciousLoginRepositoryTests()
    {
        _dbContextMock = new Mock<ZivroDbContext>();
        _repository = new SuspiciousLoginRepository(_dbContextMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuspiciousLogin()
    {
        // Arrange
        var suspiciousLoginId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com" };
        var suspiciousLogin = new SuspiciousLogin
        {
            Id = suspiciousLoginId,
            UserId = user.Id,
            Email = user.Email,
            IpAddress = "203.0.113.5",
            User = user
        };

        var mockData = new[] { suspiciousLogin }.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.SuspiciousLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(suspiciousLoginId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(suspiciousLoginId);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var mockDbSet = CreateMockDbSet(new SuspiciousLogin[0].AsQueryable());

        _dbContextMock.Setup(x => x.SuspiciousLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ReturnsNewestLogins()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var logins = new[]
        {
            new SuspiciousLogin
            {
                UserId = userId,
                Email = "user@example.com",
                IpAddress = "203.0.113.1",
                DetectedAt = now.AddMinutes(-5)
            },
            new SuspiciousLogin
            {
                UserId = userId,
                Email = "user@example.com",
                IpAddress = "203.0.113.2",
                DetectedAt = now.AddMinutes(-15)
            },
            new SuspiciousLogin
            {
                UserId = userId,
                Email = "user@example.com",
                IpAddress = "203.0.113.3",
                DetectedAt = now.AddMinutes(-25)
            }
        };

        var mockData = logins.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.SuspiciousLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByUserIdAsync(userId, limit: 2);

        // Assert
        result.Should().HaveCount(2);
        result[0].DetectedAt.Should().BeGreaterThan(result[1].DetectedAt); // Ordered newest first
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNullUserId_ReturnsRecentLogins()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var logins = new[]
        {
            new SuspiciousLogin
            {
                UserId = null, // Unregistered user attempt
                Email = "unknown@example.com",
                IpAddress = "203.0.113.1",
                DetectedAt = now
            },
            new SuspiciousLogin
            {
                UserId = Guid.NewGuid(),
                Email = "registered@example.com",
                IpAddress = "203.0.113.2",
                DetectedAt = now.AddMinutes(-10)
            }
        };

        var mockData = logins.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.SuspiciousLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByUserIdAsync(userId: null, limit: 10);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().BeNull();
    }

    [Fact]
    public async Task GetRecentByIpAndEmailAsync_WithinTimeWindow_ReturnsSuspiciousLogin()
    {
        // Arrange
        var ipAddress = "203.0.113.5";
        var email = "user@example.com";
        var now = DateTime.UtcNow;

        var suspiciousLogin = new SuspiciousLogin
        {
            Id = Guid.NewGuid(),
            IpAddress = ipAddress,
            Email = email,
            DetectedAt = now.AddMinutes(-3) // Within 5-minute window
        };

        var mockData = new[] { suspiciousLogin }.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.SuspiciousLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetRecentByIpAndEmailAsync(ipAddress, email, minutesBack: 5);

        // Assert
        result.Should().NotBeNull();
        result!.IpAddress.Should().Be(ipAddress);
    }

    [Fact]
    public async Task GetRecentByIpAndEmailAsync_OutsideTimeWindow_ReturnsNull()
    {
        // Arrange
        var ipAddress = "203.0.113.5";
        var email = "user@example.com";
        var now = DateTime.UtcNow;

        var suspiciousLogin = new SuspiciousLogin
        {
            IpAddress = ipAddress,
            Email = email,
            DetectedAt = now.AddMinutes(-10) // Outside 5-minute window
        };

        var mockData = new[] { suspiciousLogin }.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.SuspiciousLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetRecentByIpAndEmailAsync(ipAddress, email, minutesBack: 5);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFrequentLoginIpsAsync_WithUserLogins_ReturnsTopIps()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var logins = new[]
        {
            new SuspiciousLogin { UserId = userId, IpAddress = "192.0.2.1" },
            new SuspiciousLogin { UserId = userId, IpAddress = "192.0.2.1" },
            new SuspiciousLogin { UserId = userId, IpAddress = "192.0.2.1" },
            new SuspiciousLogin { UserId = userId, IpAddress = "203.0.113.5" },
            new SuspiciousLogin { UserId = userId, IpAddress = "203.0.113.5" },
            new SuspiciousLogin { UserId = Guid.NewGuid(), IpAddress = "198.51.100.1" } // Different user
        };

        var mockData = logins.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.SuspiciousLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetFrequentLoginIpsAsync(userId, limit: 5);

        // Assert
        result.Should().HaveCount(2);
        result[0].Item1.Should().Be("192.0.2.1");
        result[0].Item2.Should().Be(3);
        result[1].Item1.Should().Be("203.0.113.5");
        result[1].Item2.Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_WithValidSuspiciousLogin_AddsToRepository()
    {
        // Arrange
        var suspiciousLogin = new SuspiciousLogin
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            IpAddress = "203.0.113.5",
            Reason = new[] { SuspiciousLoginReason.NewLocation }
        };

        var mockDbSet = CreateMockDbSet(new SuspiciousLogin[0].AsQueryable());
        _dbContextMock.Setup(x => x.SuspiciousLogins).Returns(mockDbSet.Object);

        // Act
        await _repository.CreateAsync(suspiciousLogin);

        // Assert
        mockDbSet.Verify(x => x.AddAsync(suspiciousLogin, It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidSuspiciousLogin_UpdatesRepository()
    {
        // Arrange
        var suspiciousLogin = new SuspiciousLogin
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            UserNotified = true
        };

        var mockDbSet = CreateMockDbSet(new SuspiciousLogin[0].AsQueryable());
        _dbContextMock.Setup(x => x.SuspiciousLogins).Returns(mockDbSet.Object);

        // Act
        await _repository.UpdateAsync(suspiciousLogin);

        // Assert
        mockDbSet.Verify(x => x.Update(suspiciousLogin), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private Mock<DbSet<SuspiciousLogin>> CreateMockDbSet(IQueryable<SuspiciousLogin> data)
    {
        var mockDbSet = new Mock<DbSet<SuspiciousLogin>>();
        mockDbSet.As<IAsyncEnumerable<SuspiciousLogin>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new AsyncEnumerator<SuspiciousLogin>(data.GetEnumerator()));
        mockDbSet.As<IQueryable<SuspiciousLogin>>()
            .Setup(x => x.Provider)
            .Returns(data.Provider);
        mockDbSet.As<IQueryable<SuspiciousLogin>>()
            .Setup(x => x.Expression)
            .Returns(data.Expression);
        mockDbSet.As<IQueryable<SuspiciousLogin>>()
            .Setup(x => x.ElementType)
            .Returns(data.ElementType);
        mockDbSet.As<IQueryable<SuspiciousLogin>>()
            .Setup(x => x.GetEnumerator())
            .Returns(data.GetEnumerator());

        return mockDbSet;
    }
}
