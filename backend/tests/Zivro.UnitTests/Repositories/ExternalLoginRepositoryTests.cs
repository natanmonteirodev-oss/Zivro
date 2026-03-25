namespace Zivro.UnitTests.Repositories;

using Moq;
using Xunit;
using FluentAssertions;
using Zivro.Domain.Entities;
using Zivro.Infrastructure.Repositories;
using Zivro.Infrastructure;

public class ExternalLoginRepositoryTests
{
    private readonly Mock<ZivroDbContext> _dbContextMock;
    private readonly ExternalLoginRepository _repository;

    public ExternalLoginRepositoryTests()
    {
        _dbContextMock = new Mock<ZivroDbContext>();
        _repository = new ExternalLoginRepository(_dbContextMock.Object);
    }

    [Fact]
    public async Task GetByProviderAsync_WithValidUserIdAndProvider_ReturnsExternalLogin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = "Google";
        var externalLogin = new ExternalLogin
        {
            UserId = userId,
            Provider = provider,
            ProviderUserId = "google_123",
            Email = "user@gmail.com"
        };

        var mockData = new[] { externalLogin }.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.ExternalLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByProviderAsync(userId, provider);

        // Assert
        result.Should().NotBeNull();
        result!.Provider.Should().Be(provider);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByProviderAsync_NonExisting_ReturnsNull()
    {
        // Arrange
        var mockDbSet = CreateMockDbSet(new ExternalLogin[0].AsQueryable());

        _dbContextMock.Setup(x => x.ExternalLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByProviderAsync(Guid.NewGuid(), "Google");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderUserIdAsync_WithValidProviderUserId_ReturnsExternalLoginWithUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@gmail.com", Name = "User" };
        var externalLogin = new ExternalLogin
        {
            UserId = user.Id,
            Provider = "GitHub",
            ProviderUserId = "github_456",
            User = user
        };

        var mockData = new[] { externalLogin }.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.ExternalLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByProviderUserIdAsync("GitHub", "github_456");

        // Assert
        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
        result.ProviderUserId.Should().Be("github_456");
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ReturnsAllExternalLogins()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var logins = new[]
        {
            new ExternalLogin { UserId = userId, Provider = "Google", ProviderUserId = "google_1", LastLoginAt = DateTime.UtcNow.AddDays(-1) },
            new ExternalLogin { UserId = userId, Provider = "GitHub", ProviderUserId = "github_1", LastLoginAt = DateTime.UtcNow }
        };

        var mockData = logins.AsQueryable();
        var mockDbSet = CreateMockDbSet(mockData);

        _dbContextMock.Setup(x => x.ExternalLogins)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result[0].LastLoginAt.Should().BeGreaterThan(result[1].LastLoginAt); // Ordered by LastLoginAt desc
    }

    [Fact]
    public async Task CreateAsync_WithValidExternalLogin_AddsToRepository()
    {
        // Arrange
        var externalLogin = new ExternalLogin
        {
            UserId = Guid.NewGuid(),
            Provider = "Google",
            ProviderUserId = "google_123",
            Email = "user@gmail.com"
        };

        var mockDbSet = CreateMockDbSet(new ExternalLogin[0].AsQueryable());
        _dbContextMock.Setup(x => x.ExternalLogins).Returns(mockDbSet.Object);

        // Act
        await _repository.CreateAsync(externalLogin);

        // Assert
        mockDbSet.Verify(x => x.AddAsync(externalLogin, It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private Mock<DbSet<ExternalLogin>> CreateMockDbSet(IQueryable<ExternalLogin> data)
    {
        var mockDbSet = new Mock<DbSet<ExternalLogin>>();
        mockDbSet.As<IAsyncEnumerable<ExternalLogin>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new AsyncEnumerator<ExternalLogin>(data.GetEnumerator()));
        mockDbSet.As<IQueryable<ExternalLogin>>()
            .Setup(x => x.Provider)
            .Returns(data.Provider);
        mockDbSet.As<IQueryable<ExternalLogin>>()
            .Setup(x => x.Expression)
            .Returns(data.Expression);
        mockDbSet.As<IQueryable<ExternalLogin>>()
            .Setup(x => x.ElementType)
            .Returns(data.ElementType);
        mockDbSet.As<IQueryable<ExternalLogin>>()
            .Setup(x => x.GetEnumerator())
            .Returns(data.GetEnumerator());

        return mockDbSet;
    }
}

public class AsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator;

    public AsyncEnumerator(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
    }

    public T Current => _enumerator.Current;

    public async ValueTask<bool> MoveNextAsync()
    {
        return _enumerator.MoveNext();
    }

    public async ValueTask DisposeAsync()
    {
        _enumerator.Dispose();
    }
}
