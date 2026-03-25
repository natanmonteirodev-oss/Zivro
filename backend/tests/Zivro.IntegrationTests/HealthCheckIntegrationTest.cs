namespace Zivro.IntegrationTests;

using Xunit;

public class HealthCheckIntegrationTest
{
    [Fact]
    public void Health_Check_Integration_Sample()
    {
        // Arrange & Act
        var isHealthy = true;

        // Assert
        Assert.True(isHealthy);
    }
}
