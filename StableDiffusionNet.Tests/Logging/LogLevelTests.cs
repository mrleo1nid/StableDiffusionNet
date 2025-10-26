using FluentAssertions;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests.Logging
{
    /// <summary>
    /// Тесты для LogLevel enum
    /// </summary>
    public class LogLevelTests
    {
        [Fact]
        public void LogLevel_HasCorrectValues()
        {
            // Assert
            ((int)LogLevel.Debug)
                .Should()
                .Be(0);
            ((int)LogLevel.Information).Should().Be(1);
            ((int)LogLevel.Warning).Should().Be(2);
            ((int)LogLevel.Error).Should().Be(3);
            ((int)LogLevel.Critical).Should().Be(4);
        }

        [Fact]
        public void LogLevel_AllValuesAreDefined()
        {
            // Arrange
            var expectedValues = new[]
            {
                LogLevel.Debug,
                LogLevel.Information,
                LogLevel.Warning,
                LogLevel.Error,
                LogLevel.Critical,
            };

            // Act
            var allValues = Enum.GetValues<LogLevel>();

            // Assert
            allValues.Should().BeEquivalentTo(expectedValues);
        }

        [Theory]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Information, "Information")]
        [InlineData(LogLevel.Warning, "Warning")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Critical, "Critical")]
        public void LogLevel_ToString_ReturnsCorrectName(LogLevel level, string expectedName)
        {
            // Act
            var name = level.ToString();

            // Assert
            name.Should().Be(expectedName);
        }

        [Theory]
        [InlineData("Debug", LogLevel.Debug)]
        [InlineData("Information", LogLevel.Information)]
        [InlineData("Warning", LogLevel.Warning)]
        [InlineData("Error", LogLevel.Error)]
        [InlineData("Critical", LogLevel.Critical)]
        public void LogLevel_Parse_ReturnsCorrectValue(string name, LogLevel expected)
        {
            // Act
            var level = Enum.Parse<LogLevel>(name);

            // Assert
            level.Should().Be(expected);
        }

        [Fact]
        public void LogLevel_SeverityOrder_IsCorrect()
        {
            // Assert - проверяем что уровни идут от наименее серьезного к наиболее серьезному
            (LogLevel.Debug < LogLevel.Information)
                .Should()
                .BeTrue();
            (LogLevel.Information < LogLevel.Warning).Should().BeTrue();
            (LogLevel.Warning < LogLevel.Error).Should().BeTrue();
            (LogLevel.Error < LogLevel.Critical).Should().BeTrue();
        }

        [Fact]
        public void LogLevel_CanBeUsedInSwitch()
        {
            // Arrange
            var levels = new[] { LogLevel.Debug, LogLevel.Information, LogLevel.Error };
            var results = new List<string>();

            // Act
            foreach (var level in levels)
            {
                var result = level switch
                {
                    LogLevel.Debug => "debug",
                    LogLevel.Information => "info",
                    LogLevel.Warning => "warn",
                    LogLevel.Error => "error",
                    LogLevel.Critical => "critical",
                    _ => "unknown",
                };
                results.Add(result);
            }

            // Assert
            results.Should().Equal("debug", "info", "error");
        }
    }
}
