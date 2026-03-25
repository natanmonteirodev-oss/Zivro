namespace Zivro.UnitTests.Repositories;

using Moq;
using Xunit;
using FluentAssertions;
using Zivro.Domain.Entities;
using Zivro.Infrastructure.Repositories;
using Zivro.Infrastructure;

public class PasswordResetRepositoryTests
{
    private readonly Mock<ZivroDbContext> _dbContextMock;
    private readonly PasswordResetRepository _repository;

    public PasswordResetRepositoryTests()
    {
        _dbContextMock = new Mock<ZivroDbContext>();
        _repository = new PasswordResetRepository(_dbContextMock.Object);
    }

    [Fact]
    public async Task GetByTokenAsync_WithValidToken_ReturnsPasswordReset()
    {
        // Arrange
        var token = "valid_token_123";
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com" };
        var passwordReset = new PasswordReset
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = user
        };

        var mockData = new[] { passwordReset }.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.PasswordResets)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_NonExistingToken_ReturnsNull()
    {
        // Arrange
        var mockDbSet = CreateMockDbSet(new PasswordReset[0].AsQueryable());

        _dbContextMock.Setup(x => x.PasswordResets)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByTokenAsync("non_existing_token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ReturnsUnusedTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resets = new[]
        {
            new PasswordReset
            {
                UserId = userId,
                Token = "token_1",
                UsedAt = null,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            },
            new PasswordReset
            {
                UserId = userId,
                Token = "token_2",
                UsedAt = DateTime.UtcNow.AddHours(-1), // Used
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            },
            new PasswordReset
            {
                UserId = userId,
                Token = "token_3",
                UsedAt = null,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            }
        };

        var mockData = resets.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.PasswordResets)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2); // Only unused tokens
        result.Should().AllSatisfy(x => x.UsedAt.Should().BeNull());
    }

    [Fact]
    public async Task GetRecentRequestCountAsync_WithinTimeWindow_ReturnsCorrectCount()
    {
        // Arrange
        var email = "user@example.com";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = email };

        var now = DateTime.UtcNow;
        var resets = new[]
        {
            new PasswordReset
            {
                UserId = userId,
                Token = "token_1",
                CreatedAt = now.AddMinutes(-30),
                User = user
            },
            new PasswordReset
            {
                UserId = userId,
                Token = "token_2",
                CreatedAt = now.AddMinutes(-45),
                User = user
            },
            new PasswordReset
            {
                UserId = userId,
                Token = "token_3",
                CreatedAt = now.AddMinutes(-90), // Outside 60-minute window
                User = user
            }
        };

        var mockData = resets.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.PasswordResets)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetRecentRequestCountAsync(email, minutesBack: 60);

        // Assert
        result.Should().Be(2); // Only tokens created within last 60 minutes
    }

    [Fact]
    public async Task CreateAsync_WithValidPasswordReset_AddsToRepository()
    {
        // Arrange
        var passwordReset = new PasswordReset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "new_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var mockDbSet = CreateMockDbSet(new PasswordReset[0].AsQueryable());
        _dbContextMock.Setup(x => x.PasswordResets).Returns(mockDbSet.Object);

        // Act
        await _repository.CreateAsync(passwordReset);

        // Assert
        mockDbSet.Verify(x => x.AddAsync(passwordReset, It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidPasswordReset_UpdatesRepository()
    {
        // Arrange
        var passwordReset = new PasswordReset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "token",
            UsedAt = DateTime.UtcNow
        };

        var mockDbSet = CreateMockDbSet(new PasswordReset[0].AsQueryable());
        _dbContextMock.Setup(x => x.PasswordResets).Returns(mockDbSet.Object);

        // Act
        await _repository.UpdateAsync(passwordReset);

        // Assert
        mockDbSet.Verify(x => x.Update(passwordReset), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private Mock<DbSet<PasswordReset>> CreateMockDbSet(IQueryable<PasswordReset> data)
    {
        var mockDbSet = new Mock<DbSet<PasswordReset>>();
        mockDbSet.As<IAsyncEnumerable<PasswordReset>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new AsyncEnumerator<PasswordReset>(data.GetEnumerator()));
        mockDbSet.As<IQueryable<PasswordReset>>()
            .Setup(x => x.Provider)
            .Returns(data.Provider);
        mockDbSet.As<IQueryable<PasswordReset>>()
            .Setup(x => x.Expression)
            .Returns(data.Expression);
        mockDbSet.As<IQueryable<PasswordReset>>()
            .Setup(x => x.ElementType)
            .Returns(data.ElementType);
        mockDbSet.As<IQueryable<PasswordReset>>()
            .Setup(x => x.GetEnumerator())
            .Returns(data.GetEnumerator());

        return mockDbSet;
    }
}
