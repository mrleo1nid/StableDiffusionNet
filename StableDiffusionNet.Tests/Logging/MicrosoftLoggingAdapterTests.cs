using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StableDiffusionNet.DependencyInjection.Logging;
using MsftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using SdLogLevel = StableDiffusionNet.Logging.LogLevel;

namespace StableDiffusionNet.Tests.Logging
{
    /// <summary>
    /// Тесты для MicrosoftLoggingAdapter
    /// </summary>
    public class MicrosoftLoggingAdapterTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly MicrosoftLoggingAdapter _adapter;

        public MicrosoftLoggingAdapterTests()
        {
            _mockLogger = new Mock<ILogger>();
            _adapter = new MicrosoftLoggingAdapter(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new MicrosoftLoggingAdapter(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Theory]
        [InlineData(SdLogLevel.Debug, MsftLogLevel.Debug)]
        [InlineData(SdLogLevel.Information, MsftLogLevel.Information)]
        [InlineData(SdLogLevel.Warning, MsftLogLevel.Warning)]
        [InlineData(SdLogLevel.Error, MsftLogLevel.Error)]
        [InlineData(SdLogLevel.Critical, MsftLogLevel.Critical)]
        public void Log_WithMessage_CallsUnderlyingLoggerWithCorrectLevel(
            SdLogLevel sdLevel,
            MsftLogLevel expectedMsftLevel
        )
        {
            // Arrange
            var message = "test message";

            // Act
            _adapter.Log(sdLevel, message);

            // Assert
            _mockLogger.Verify(
                x =>
                    x.Log(
                        expectedMsftLevel,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Theory]
        [InlineData(SdLogLevel.Debug, MsftLogLevel.Debug)]
        [InlineData(SdLogLevel.Information, MsftLogLevel.Information)]
        [InlineData(SdLogLevel.Warning, MsftLogLevel.Warning)]
        [InlineData(SdLogLevel.Error, MsftLogLevel.Error)]
        [InlineData(SdLogLevel.Critical, MsftLogLevel.Critical)]
        public void Log_WithException_CallsUnderlyingLoggerWithCorrectLevel(
            SdLogLevel sdLevel,
            MsftLogLevel expectedMsftLevel
        )
        {
            // Arrange
            var message = "test message";
            var exception = new InvalidOperationException("test exception");

            // Act
            _adapter.Log(sdLevel, exception, message);

            // Assert
            _mockLogger.Verify(
                x =>
                    x.Log(
                        expectedMsftLevel,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public void Log_WithNullException_DoesNotThrow()
        {
            // Arrange
            var message = "test message";

            // Act
            var act = () => _adapter.Log(SdLogLevel.Error, null!, message);

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(SdLogLevel.Debug, MsftLogLevel.Debug)]
        [InlineData(SdLogLevel.Information, MsftLogLevel.Information)]
        [InlineData(SdLogLevel.Warning, MsftLogLevel.Warning)]
        [InlineData(SdLogLevel.Error, MsftLogLevel.Error)]
        [InlineData(SdLogLevel.Critical, MsftLogLevel.Critical)]
        public void IsEnabled_ReturnsUnderlyingLoggerValue(
            SdLogLevel sdLevel,
            MsftLogLevel expectedMsftLevel
        )
        {
            // Arrange
            _mockLogger.Setup(x => x.IsEnabled(expectedMsftLevel)).Returns(true);

            // Act
            var result = _adapter.IsEnabled(sdLevel);

            // Assert
            result.Should().BeTrue();
            _mockLogger.Verify(x => x.IsEnabled(expectedMsftLevel), Times.Once);
        }

        [Fact]
        public void IsEnabled_WhenUnderlyingReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _mockLogger.Setup(x => x.IsEnabled(It.IsAny<MsftLogLevel>())).Returns(false);

            // Act
            var result = _adapter.IsEnabled(SdLogLevel.Debug);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Log_WithEmptyMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var message = string.Empty;

            // Act
            _adapter.Log(SdLogLevel.Information, message);

            // Assert
            _mockLogger.Verify(
                x =>
                    x.Log(
                        MsftLogLevel.Information,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public void Log_WithLongMessage_CallsUnderlyingLogger()
        {
            // Arrange
            var message = new string('a', 10000);

            // Act
            _adapter.Log(SdLogLevel.Information, message);

            // Assert
            _mockLogger.Verify(
                x =>
                    x.Log(
                        MsftLogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public void Log_MultipleCallsWithDifferentLevels_AllCallsForwarded()
        {
            // Act
            _adapter.Log(SdLogLevel.Debug, "debug");
            _adapter.Log(SdLogLevel.Information, "info");
            _adapter.Log(SdLogLevel.Warning, "warn");
            _adapter.Log(SdLogLevel.Error, "error");
            _adapter.Log(SdLogLevel.Critical, "critical");

            // Assert
            _mockLogger.Verify(
                x =>
                    x.Log(
                        It.IsAny<MsftLogLevel>(),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Exactly(5)
            );
        }

        [Fact]
        public void Log_WithExceptionAndMessage_BothAreForwarded()
        {
            // Arrange
            var message = "error occurred";
            var exception = new ArgumentException("invalid argument");

            // Act
            _adapter.Log(SdLogLevel.Error, exception, message);

            // Assert
            _mockLogger.Verify(
                x =>
                    x.Log(
                        MsftLogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public void IsEnabled_CheckedMultipleTimes_AlwaysCallsUnderlyingLogger()
        {
            // Arrange
            _mockLogger.Setup(x => x.IsEnabled(It.IsAny<MsftLogLevel>())).Returns(true);

            // Act
            _adapter.IsEnabled(SdLogLevel.Debug);
            _adapter.IsEnabled(SdLogLevel.Debug);
            _adapter.IsEnabled(SdLogLevel.Debug);

            // Assert
            _mockLogger.Verify(x => x.IsEnabled(MsftLogLevel.Debug), Times.Exactly(3));
        }
    }
}
