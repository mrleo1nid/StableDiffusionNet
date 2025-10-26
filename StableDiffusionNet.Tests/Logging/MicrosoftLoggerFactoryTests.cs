using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StableDiffusionNet.DependencyInjection.Logging;
using StableDiffusionNet.Logging;
using IStableDiffusionLogger = StableDiffusionNet.Logging.IStableDiffusionLogger;

namespace StableDiffusionNet.Tests.Logging
{
    /// <summary>
    /// Тесты для MicrosoftLoggerFactory
    /// </summary>
    public class MicrosoftLoggerFactoryTests
    {
        [Fact]
        public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new MicrosoftLoggerFactory(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("loggerFactory");
        }

        [Fact]
        public void CreateLogger_Generic_ReturnsAdapter()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<MicrosoftLoggerFactoryTests>>();

            mockLoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(mockLogger.Object);

            var factory = new MicrosoftLoggerFactory(mockLoggerFactory.Object);

            // Act
            var logger = factory.CreateLogger<MicrosoftLoggerFactoryTests>();

            // Assert
            logger.Should().NotBeNull();
            logger.Should().BeAssignableTo<IStableDiffusionLogger>();
            mockLoggerFactory.Verify(
                x => x.CreateLogger("StableDiffusionNet.Tests.Logging.MicrosoftLoggerFactoryTests"),
                Times.Once
            );
        }

        [Fact]
        public void CreateLogger_WithCategoryName_ReturnsAdapter()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            mockLoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(mockLogger.Object);

            var factory = new MicrosoftLoggerFactory(mockLoggerFactory.Object);

            // Act
            var logger = factory.CreateLogger("TestCategory");

            // Assert
            logger.Should().NotBeNull();
            logger.Should().BeAssignableTo<IStableDiffusionLogger>();
            mockLoggerFactory.Verify(x => x.CreateLogger("TestCategory"), Times.Once);
        }

        [Fact]
        public void CreateLogger_WithEmptyCategory_CreatesLogger()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            mockLoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(mockLogger.Object);

            var factory = new MicrosoftLoggerFactory(mockLoggerFactory.Object);

            // Act
            var logger = factory.CreateLogger(string.Empty);

            // Assert
            logger.Should().NotBeNull();
            mockLoggerFactory.Verify(x => x.CreateLogger(string.Empty), Times.Once);
        }

        [Fact]
        public void CreateLogger_MultipleCalls_CreatesDifferentAdapters()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger1 = new Mock<ILogger>();
            var mockLogger2 = new Mock<ILogger>();

            mockLoggerFactory
                .SetupSequence(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(mockLogger1.Object)
                .Returns(mockLogger2.Object);

            var factory = new MicrosoftLoggerFactory(mockLoggerFactory.Object);

            // Act
            var logger1 = factory.CreateLogger("Category1");
            var logger2 = factory.CreateLogger("Category2");

            // Assert
            logger1.Should().NotBeNull();
            logger2.Should().NotBeNull();
            logger1.Should().NotBeSameAs(logger2, "каждый вызов должен создавать новый адаптер");
        }

        [Fact]
        public void CreateLogger_DifferentTypes_UsesCorrectCategoryNames()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            mockLoggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(mockLogger.Object);

            var factory = new MicrosoftLoggerFactory(mockLoggerFactory.Object);

            // Act
            factory.CreateLogger<MicrosoftLoggerFactoryTests>();
            factory.CreateLogger<string>();

            // Assert
            mockLoggerFactory.Verify(
                x => x.CreateLogger("StableDiffusionNet.Tests.Logging.MicrosoftLoggerFactoryTests"),
                Times.Once
            );
            // Microsoft LoggerFactory использует TypeNameHelper который возвращает "string" вместо "System.String"
            mockLoggerFactory.Verify(x => x.CreateLogger("string"), Times.Once);
        }
    }
}
