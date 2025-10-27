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
            // Arrange & Act
            var options = new StableDiffusionOptions();
            var act = () => options.BaseUrl = string.Empty;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*")
                .WithParameterName("BaseUrl");
        }

        [Fact]
        public void Validate_WithNullBaseUrl_ThrowsConfigurationException()
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();
            var act = () => options.BaseUrl = null!;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*")
                .WithParameterName("BaseUrl");
        }

        [Fact]
        public void Validate_WithWhitespaceBaseUrl_ThrowsConfigurationException()
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();
            var act = () => options.BaseUrl = "   ";

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*")
                .WithParameterName("BaseUrl");
        }

        [Fact]
        public void Validate_WithInvalidBaseUrl_ThrowsConfigurationException()
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();
            var act = () => options.BaseUrl = "not a valid url";

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*BaseUrl must be a valid URL*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WithNonPositiveTimeout_ThrowsConfigurationException(int timeout)
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();
            var act = () => options.TimeoutSeconds = timeout;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*TimeoutSeconds must be a positive number*");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_WithNegativeRetryCount_ThrowsConfigurationException(int retryCount)
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();
            var act = () => options.RetryCount = retryCount;

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*RetryCount cannot be negative*");
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
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-1000)]
        public void Validate_WithNegativeRetryDelayMilliseconds_ThrowsConfigurationException(
            int retryDelay
        )
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();
            var act = () => options.RetryDelayMilliseconds = retryDelay;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*RetryDelayMilliseconds cannot be negative*");
        }

        [Fact]
        public void Validate_WithZeroRetryDelayMilliseconds_DoesNotThrow()
        {
            // Arrange
            var options = new StableDiffusionOptions { RetryDelayMilliseconds = 0 };

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

        [Fact]
        public void BaseUrl_Setter_WithEmptyString_ThrowsArgumentException()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            var act = () => options.BaseUrl = string.Empty;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("BaseUrl")
                .WithMessage("*cannot be null, empty, or whitespace*");
        }

        [Fact]
        public void BaseUrl_Setter_WithInvalidUrl_ThrowsArgumentException()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            var act = () => options.BaseUrl = "invalid-url";

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("BaseUrl")
                .WithMessage("*BaseUrl must be a valid URL*");
        }

        [Fact]
        public void TimeoutSeconds_Setter_WithZero_ThrowsArgumentException()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            var act = () => options.TimeoutSeconds = 0;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("TimeoutSeconds")
                .WithMessage("*TimeoutSeconds must be a positive number*");
        }

        [Fact]
        public void TimeoutSeconds_Setter_WithNegative_ThrowsArgumentException()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            var act = () => options.TimeoutSeconds = -1;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("TimeoutSeconds")
                .WithMessage("*TimeoutSeconds must be a positive number*");
        }

        [Fact]
        public void RetryCount_Setter_WithNegative_ThrowsArgumentException()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            var act = () => options.RetryCount = -1;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("RetryCount")
                .WithMessage("*RetryCount cannot be negative*");
        }

        [Fact]
        public void RetryDelayMilliseconds_Setter_WithNegative_ThrowsArgumentException()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            var act = () => options.RetryDelayMilliseconds = -1;

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("RetryDelayMilliseconds")
                .WithMessage("*RetryDelayMilliseconds cannot be negative*");
        }

        [Fact]
        public void BaseUrl_Setter_WithValidUrl_SetsValue()
        {
            // Arrange
            var options = new StableDiffusionOptions();
            var newUrl = "https://api.example.com:8080";

            // Act
            options.BaseUrl = newUrl;

            // Assert
            options.BaseUrl.Should().Be(newUrl);
        }

        [Fact]
        public void RetryCount_Setter_WithZero_SetsValue()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.RetryCount = 0;

            // Assert
            options.RetryCount.Should().Be(0);
        }

        [Fact]
        public void RetryCount_Setter_WithPositive_SetsValue()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.RetryCount = 5;

            // Assert
            options.RetryCount.Should().Be(5);
        }

        [Fact]
        public void RetryDelayMilliseconds_Setter_WithZero_SetsValue()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.RetryDelayMilliseconds = 0;

            // Assert
            options.RetryDelayMilliseconds.Should().Be(0);
        }

        [Fact]
        public void Validate_WithAllPropertiesSet_DoesNotThrow()
        {
            // Arrange
            var options = new StableDiffusionOptions
            {
                BaseUrl = "https://api.example.com",
                TimeoutSeconds = 600,
                RetryCount = 5,
                RetryDelayMilliseconds = 2000,
                ApiKey = "test-key",
                EnableDetailedLogging = true,
            };

            // Act & Assert
            var act = () => options.Validate();
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(60)]
        [InlineData(300)]
        [InlineData(3600)]
        [InlineData(int.MaxValue)]
        public void TimeoutSeconds_Setter_WithValidPositiveValues_SetsValue(int timeout)
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.TimeoutSeconds = timeout;

            // Assert
            options.TimeoutSeconds.Should().Be(timeout);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void RetryCount_Setter_WithValidNonNegativeValues_SetsValue(int retryCount)
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.RetryCount = retryCount;

            // Assert
            options.RetryCount.Should().Be(retryCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(int.MaxValue)]
        public void RetryDelayMilliseconds_Setter_WithValidNonNegativeValues_SetsValue(int delay)
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.RetryDelayMilliseconds = delay;

            // Assert
            options.RetryDelayMilliseconds.Should().Be(delay);
        }

        [Fact]
        public void ApiKey_DefaultValue_IsNull()
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();

            // Assert
            options.ApiKey.Should().BeNull();
        }

        [Fact]
        public void ApiKey_CanBeSetToEmptyString()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act
            options.ApiKey = string.Empty;

            // Assert
            options.ApiKey.Should().Be(string.Empty);
        }

        [Fact]
        public void EnableDetailedLogging_DefaultValue_IsFalse()
        {
            // Arrange & Act
            var options = new StableDiffusionOptions();

            // Assert
            options.EnableDetailedLogging.Should().BeFalse();
        }

        [Fact]
        public void EnableDetailedLogging_CanBeToggledMultipleTimes()
        {
            // Arrange
            var options = new StableDiffusionOptions();

            // Act & Assert
            options.EnableDetailedLogging = true;
            options.EnableDetailedLogging.Should().BeTrue();

            options.EnableDetailedLogging = false;
            options.EnableDetailedLogging.Should().BeFalse();

            options.EnableDetailedLogging = true;
            options.EnableDetailedLogging.Should().BeTrue();
        }

        #region Validate Method Tests (для покрытия ConfigurationException при десериализации)

        [Fact]
        public void Validate_WithEmptyBaseUrl_ViaReflection_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions();
            var field = typeof(StableDiffusionOptions).GetField(
                "_baseUrl",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            field!.SetValue(options, string.Empty);

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*cannot be null, empty, or whitespace*");
        }

        [Fact]
        public void Validate_WithInvalidBaseUrl_ViaReflection_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions();
            var field = typeof(StableDiffusionOptions).GetField(
                "_baseUrl",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            field!.SetValue(options, "invalid-url");

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*BaseUrl must be a valid URL*");
        }

        [Fact]
        public void Validate_WithNonPositiveTimeout_ViaReflection_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions();
            var field = typeof(StableDiffusionOptions).GetField(
                "_timeoutSeconds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            field!.SetValue(options, 0);

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*TimeoutSeconds must be a positive number*");
        }

        [Fact]
        public void Validate_WithNegativeRetryCount_ViaReflection_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions();
            var field = typeof(StableDiffusionOptions).GetField(
                "_retryCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            field!.SetValue(options, -1);

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*RetryCount cannot be negative*");
        }

        [Fact]
        public void Validate_WithNegativeRetryDelay_ViaReflection_ThrowsConfigurationException()
        {
            // Arrange
            var options = new StableDiffusionOptions();
            var field = typeof(StableDiffusionOptions).GetField(
                "_retryDelayMilliseconds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            field!.SetValue(options, -1);

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should()
                .Throw<ConfigurationException>()
                .WithMessage("*RetryDelayMilliseconds cannot be negative*");
        }

        #endregion
    }
}
