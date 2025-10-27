using FluentAssertions;
using Moq;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests
{
    /// <summary>
    /// Тесты для StableDiffusionClientBuilder
    /// </summary>
    public class StableDiffusionClientBuilderTests
    {
        [Fact]
        public void CreateDefault_CreatesClient_Successfully()
        {
            // Act
            var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");

            // Assert
            client.Should().NotBeNull();
            client.Should().BeAssignableTo<IStableDiffusionClient>();
            client.TextToImage.Should().NotBeNull();
            client.ImageToImage.Should().NotBeNull();
            client.Models.Should().NotBeNull();
            client.Progress.Should().NotBeNull();
            client.Options.Should().NotBeNull();
            client.Samplers.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithCustomOptions_CreatesClient_Successfully()
        {
            // Arrange
            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithTimeout(600)
                .WithRetry(5, 2000)
                .WithDetailedLogging();

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
            client.Should().BeAssignableTo<IStableDiffusionClient>();
        }

        [Fact]
        public void Build_WithLoggerFactory_CreatesClient_Successfully()
        {
            // Arrange
            var logger = Mock.Of<IStableDiffusionLogger>();
            var loggerFactoryMock = new Mock<IStableDiffusionLoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger);
            loggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(logger);

            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithLoggerFactory(loggerFactoryMock.Object);

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithApiKey_CreatesClient_Successfully()
        {
            // Arrange
            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithApiKey("test-api-key");

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithStableDiffusionOptions_CreatesClient_Successfully()
        {
            // Arrange
            var options = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                TimeoutSeconds = 600,
                RetryCount = 5,
            };

            var builder = new StableDiffusionClientBuilder().WithOptions(options);

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithInvalidBaseUrl_ThrowsException()
        {
            // Arrange & Act
            Action act = () => new StableDiffusionClientBuilder().WithBaseUrl("invalid-url");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*BaseUrl must be a valid URL*");
        }

        [Fact]
        public void Build_WithEmptyBaseUrl_ThrowsException()
        {
            // Arrange & Act
            Action act = () => new StableDiffusionClientBuilder().WithBaseUrl("");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*BaseUrl cannot be empty*");
        }

        [Fact]
        public void Build_WithNegativeTimeout_ThrowsException()
        {
            // Arrange & Act
            Action act = () =>
                new StableDiffusionClientBuilder()
                    .WithBaseUrl("http://localhost:7860")
                    .WithTimeout(-1);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*TimeoutSeconds must be a positive number*");
        }

        [Fact]
        public void Build_WithNegativeRetryCount_ThrowsException()
        {
            // Arrange & Act
            Action act = () =>
                new StableDiffusionClientBuilder()
                    .WithBaseUrl("http://localhost:7860")
                    .WithRetry(-1);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*RetryCount cannot be negative*");
        }

        [Fact]
        public void WithOptions_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new StableDiffusionClientBuilder();

            // Act
            Action act = () => builder.WithOptions(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }
    }
}
