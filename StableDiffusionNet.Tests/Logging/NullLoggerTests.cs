using FluentAssertions;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests.Logging
{
    /// <summary>
    /// Тесты для NullLogger
    /// </summary>
    public class NullLoggerTests
    {
        [Fact]
        public void Instance_ReturnsSameInstance()
        {
            // Act
            var instance1 = NullLogger.Instance;
            var instance2 = NullLogger.Instance;

            // Assert
            instance1.Should().BeSameAs(instance2, "NullLogger должен быть синглтоном");
        }

        [Fact]
        public void Log_WithMessage_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () => logger.Log(LogLevel.Information, "test message");

            // Assert
            act.Should().NotThrow("NullLogger не должен выбрасывать исключения");
        }

        [Fact]
        public void Log_WithException_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;
            var exception = new Exception("test exception");

            // Act
            var act = () => logger.Log(LogLevel.Error, exception, "test message");

            // Assert
            act.Should().NotThrow("NullLogger не должен выбрасывать исключения");
        }

        [Fact]
        public void Log_WithNullException_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () => logger.Log(LogLevel.Error, null!, "test message");

            // Assert
            act.Should().NotThrow("NullLogger не должен выбрасывать исключения даже с null");
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void IsEnabled_AlwaysReturnsFalse(LogLevel logLevel)
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var result = logger.IsEnabled(logLevel);

            // Assert
            result.Should().BeFalse("NullLogger всегда отключен");
        }

        [Fact]
        public void LogDebug_ExtensionMethod_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () => logger.LogDebug("debug message");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void LogInformation_ExtensionMethod_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () => logger.LogInformation("info message");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void LogWarning_ExtensionMethod_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () => logger.LogWarning("warning message");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void LogError_ExtensionMethod_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () => logger.LogError("error message");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void LogError_WithException_ExtensionMethod_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;
            var exception = new InvalidOperationException("test");

            // Act
            var act = () => logger.LogError(exception, "error message");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void LogCritical_ExtensionMethod_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () => logger.LogCritical("critical message");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void LogCritical_WithException_ExtensionMethod_DoesNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;
            var exception = new InvalidOperationException("test");

            // Act
            var act = () => logger.LogCritical(exception, "critical message");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void MultipleLogCalls_DoNotInterfere()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act
            var act = () =>
            {
                logger.LogDebug("message 1");
                logger.LogInformation("message 2");
                logger.LogWarning("message 3");
                logger.LogError("message 4");
                logger.LogCritical("message 5");
            };

            // Assert
            act.Should().NotThrow("множественные вызовы не должны конфликтовать");
        }
    }
}
