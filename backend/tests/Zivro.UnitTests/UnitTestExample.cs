namespace Zivro.UnitTests;

using Xunit;

public class UnitTestExample
{
    [Fact]
    public void Sample_Test_Should_Pass()
    {
        // Arrange
        var expected = 2;

        // Act
        var result = 1 + 1;

        // Assert
        Assert.Equal(expected, result);
    }
}
