using FluentAssertions;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Exceptions;

namespace StableDiffusionNet.Tests.Configuration
{
    /// <summary>
    /// Тесты для StableDiffusionOptions
    /// </summary>
    public class StableDiffusionOptionsTests
    {
        [Fact]
        public void DefaultValues_ShouldBeValid()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Assert
            options.BaseUrl.Should().Be("http://localhost:7860");
            options.TimeoutSeconds.Should().Be(300);
            options.RetryCount.Should().Be(3);
            options.RetryDelayMilliseconds.Should().Be(1000);
            options.ApiKey.Should().BeNull();
            options.EnableDetailedLogging.Should().BeFalse();
        }

        [Fact]
        public void Validate_WithValidOptions_DoesNotThrow()
        {
            // Arrange
            var options = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                TimeoutSeconds = 300,
                RetryCount = 3,
            };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_WithEmptyBaseUrl_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions { BaseUrl = string.Empty };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().Throw<ConfigurationException>().WithMessage("*BaseUrl cannot be empty*");
        }

        [Fact]
        public void Validate_WithNullBaseUrl_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions { BaseUrl = null! };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().Throw<ConfigurationException>().WithMessage("*BaseUrl cannot be empty*");
        }

        [Fact]
        public void Validate_WithWhitespaceBaseUrl_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions { BaseUrl = "   " };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().Throw<ConfigurationException>().WithMessage("*BaseUrl cannot be empty*");
        }

        [Fact]
        public void Validate_WithInvalidBaseUrl_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions { BaseUrl = "not a valid url" };

            // Act & Assert
            var act = () => options.Validate();
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*BaseUrl must be a valid URL*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNonPositiveTimeout_ThrowsConfigurationException(int timeout)
        {
            // Arrange
            var options = new StableDiffusionOptions { TimeoutSeconds = timeout };

            // Act & Assert
            var act = () => options.Validate();
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*TimeoutSeconds must be a positive number*");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_WithNegativeRetryCount_ThrowsConfigurationException(int retryCount)
        {
            // Arrange
            var options = new StableDiffusionOptions { RetryCount = retryCount };

            // Act & Assert
            var act = () => options.Validate();
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*RetryCount cannot be negative*");
        }

        [Fact]
        public void Validate_WithZeroRetryCount_DoesNotThrow()
        {
            // Arrange
            var options = new StableDiffusionOptions { RetryCount = 0 };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("http://localhost:7860")]
        [InlineData("https://example.com")]
        [InlineData("http://192.168.1.1:8080")]
        [InlineData("https://api.example.com/path")]
        public void Validate_WithValidUrls_DoesNotThrow(string url)
        {
            // Arrange
            var options = new StableDiffusionOptions { BaseUrl = url };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Fact]
        public void ApiKey_CanBeSet()
        {
            // Arrange
            var options = new StableDiffusionOptions();
            var apiKey = "test-api-key-12345";

            // Act
            options.ApiKey = apiKey;

            // Assert
            options.ApiKey.Should().Be(apiKey);
        }

        [Fact]
        public void EnableDetailedLogging_CanBeEnabled()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.EnableDetailedLogging = true;

            // Assert
            options.EnableDetailedLogging.Should().BeTrue();
        }

        [Fact]
        public void CustomTimeout_CanBeSet()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.TimeoutSeconds = 600;

            // Assert
            options.TimeoutSeconds.Should().Be(600);
        }

        [Fact]
        public void RetryDelayMilliseconds_CanBeSet()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.RetryDelayMilliseconds = 2000;

            // Assert
            options.RetryDelayMilliseconds.Should().Be(2000);
        }
    }
}
