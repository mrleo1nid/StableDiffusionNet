using FluentAssertions;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests.Logging
{
    /// <summary>
    /// Тесты для NullLoggerFactory
    /// </summary>
    public class NullLoggerFactoryTests
    {
        [Fact]
        public void Instance_ReturnsSameInstance()
        {
            // Act
            var instance1 = NullLoggerFactory.Instance;
            var instance2 = NullLoggerFactory.Instance;

            // Assert
            instance1.Should().BeSameAs(instance2, "NullLoggerFactory должна быть синглтоном");
        }

        [Fact]
        public void CreateLogger_Generic_ReturnsNullLogger()
        {
            // Arrange
            var factory = NullLoggerFactory.Instance;

            // Act
            var logger = factory.CreateLogger<NullLoggerFactoryTests>();

            // Assert
            logger.Should().NotBeNull();
            logger
                .Should()
                .BeSameAs(NullLogger.Instance, "фабрика должна возвращать NullLogger.Instance");
        }

        [Fact]
        public void CreateLogger_WithCategoryName_ReturnsNullLogger()
        {
            // Arrange
            var factory = NullLoggerFactory.Instance;

            // Act
            var logger = factory.CreateLogger("TestCategory");

            // Assert
            logger.Should().NotBeNull();
            logger
                .Should()
                .BeSameAs(NullLogger.Instance, "фабрика должна возвращать NullLogger.Instance");
        }

        [Fact]
        public void CreateLogger_WithEmptyCategory_ReturnsNullLogger()
        {
            // Arrange
            var factory = NullLoggerFactory.Instance;

            // Act
            var logger = factory.CreateLogger(string.Empty);

            // Assert
            logger.Should().NotBeNull();
            logger.Should().BeSameAs(NullLogger.Instance);
        }

        [Fact]
        public void CreateLogger_WithNullCategory_ReturnsNullLogger()
        {
            // Arrange
            var factory = NullLoggerFactory.Instance;

            // Act
            var logger = factory.CreateLogger(null!);

            // Assert
            logger.Should().NotBeNull();
            logger.Should().BeSameAs(NullLogger.Instance);
        }

        [Fact]
        public void CreateLogger_MultipleCalls_ReturnsSameInstance()
        {
            // Arrange
            var factory = NullLoggerFactory.Instance;

            // Act
            var logger1 = factory.CreateLogger<NullLoggerFactoryTests>();
            var logger2 = factory.CreateLogger<NullLoggerFactoryTests>();
            var logger3 = factory.CreateLogger("Category1");
            var logger4 = factory.CreateLogger("Category2");

            // Assert
            logger1.Should().BeSameAs(logger2);
            logger2.Should().BeSameAs(logger3);
            logger3.Should().BeSameAs(logger4);
            logger4
                .Should()
                .BeSameAs(NullLogger.Instance, "все логгеры должны быть одним экземпляром");
        }

        [Fact]
        public void CreateLogger_DifferentTypes_ReturnsSameLogger()
        {
            // Arrange
            var factory = NullLoggerFactory.Instance;

            // Act
            var logger1 = factory.CreateLogger<NullLoggerFactoryTests>();
            var logger2 = factory.CreateLogger<NullLoggerTests>();
            var logger3 = factory.CreateLogger<string>();

            // Assert
            logger1.Should().BeSameAs(logger2);
            logger2.Should().BeSameAs(logger3);
        }
    }
}
