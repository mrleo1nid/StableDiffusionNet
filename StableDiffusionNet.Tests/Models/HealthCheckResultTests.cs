using FluentAssertions;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Tests.Models
{
    /// <summary>
    /// Тесты для модели HealthCheckResult
    /// </summary>
    public class HealthCheckResultTests
    {
        [Fact]
        public void Constructor_DefaultValues_InitializesCorrectly()
        {
            // Act
            var result = new HealthCheckResult();

            // Assert
            result.IsHealthy.Should().BeFalse();
            result.ResponseTime.Should().BeNull();
            result.Error.Should().BeNull();
            result.CheckedAt.Should().Be(default(DateTime));
            result.Endpoint.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithValues_InitializesCorrectly()
        {
            // Arrange
            var responseTime = TimeSpan.FromMilliseconds(150);
            var checkedAt = DateTime.UtcNow;
            var endpoint = "/test/endpoint";
            var error = "Test error";

            // Act
            var result = new HealthCheckResult
            {
                IsHealthy = true,
                ResponseTime = responseTime,
                Error = error,
                CheckedAt = checkedAt,
                Endpoint = endpoint,
            };

            // Assert
            result.IsHealthy.Should().BeTrue();
            result.ResponseTime.Should().Be(responseTime);
            result.Error.Should().Be(error);
            result.CheckedAt.Should().Be(checkedAt);
            result.Endpoint.Should().Be(endpoint);
        }

        [Fact]
        public void Success_WithResponseTime_CreatesHealthyResult()
        {
            // Arrange
            var responseTime = TimeSpan.FromMilliseconds(200);
            var endpoint = "/api/health";

            // Act
            var result = HealthCheckResult.Success(responseTime, endpoint);

            // Assert
            result.IsHealthy.Should().BeTrue();
            result.ResponseTime.Should().Be(responseTime);
            result.Error.Should().BeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Endpoint.Should().Be(endpoint);
        }

        [Fact]
        public void Success_WithResponseTimeOnly_CreatesHealthyResult()
        {
            // Arrange
            var responseTime = TimeSpan.FromMilliseconds(100);

            // Act
            var result = HealthCheckResult.Success(responseTime);

            // Assert
            result.IsHealthy.Should().BeTrue();
            result.ResponseTime.Should().Be(responseTime);
            result.Error.Should().BeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Endpoint.Should().BeNull();
        }

        [Fact]
        public void Failure_WithError_CreatesUnhealthyResult()
        {
            // Arrange
            var error = "Connection timeout";
            var endpoint = "/api/health";

            // Act
            var result = HealthCheckResult.Failure(error, endpoint);

            // Assert
            result.IsHealthy.Should().BeFalse();
            result.ResponseTime.Should().BeNull();
            result.Error.Should().Be(error);
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Endpoint.Should().Be(endpoint);
        }

        [Fact]
        public void Failure_WithErrorOnly_CreatesUnhealthyResult()
        {
            // Arrange
            var error = "Service unavailable";

            // Act
            var result = HealthCheckResult.Failure(error);

            // Assert
            result.IsHealthy.Should().BeFalse();
            result.ResponseTime.Should().BeNull();
            result.Error.Should().Be(error);
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Endpoint.Should().BeNull();
        }

        [Fact]
        public void Failure_WithEmptyError_CreatesUnhealthyResult()
        {
            // Arrange
            var error = "";

            // Act
            var result = HealthCheckResult.Failure(error);

            // Assert
            result.IsHealthy.Should().BeFalse();
            result.ResponseTime.Should().BeNull();
            result.Error.Should().Be(error);
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Endpoint.Should().BeNull();
        }

        [Fact]
        public void Failure_WithNullError_CreatesUnhealthyResult()
        {
            // Arrange
            string? error = null;

            // Act
            var result = HealthCheckResult.Failure(error!);

            // Assert
            result.IsHealthy.Should().BeFalse();
            result.ResponseTime.Should().BeNull();
            result.Error.Should().BeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Endpoint.Should().BeNull();
        }

        [Fact]
        public void Record_Equality_WorksCorrectly()
        {
            // Arrange
            var responseTime = TimeSpan.FromMilliseconds(150);
            var checkedAt = DateTime.UtcNow;
            var endpoint = "/test/endpoint";

            var result1 = new HealthCheckResult
            {
                IsHealthy = true,
                ResponseTime = responseTime,
                CheckedAt = checkedAt,
                Endpoint = endpoint,
            };

            var result2 = new HealthCheckResult
            {
                IsHealthy = true,
                ResponseTime = responseTime,
                CheckedAt = checkedAt,
                Endpoint = endpoint,
            };

            // Act & Assert
            result1.Should().Be(result2);
            result1.GetHashCode().Should().Be(result2.GetHashCode());
        }

        [Fact]
        public void Record_Inequality_WorksCorrectly()
        {
            // Arrange
            var result1 = new HealthCheckResult
            {
                IsHealthy = true,
                ResponseTime = TimeSpan.FromMilliseconds(100),
            };

            var result2 = new HealthCheckResult
            {
                IsHealthy = false,
                ResponseTime = TimeSpan.FromMilliseconds(200),
            };

            // Act & Assert
            result1.Should().NotBe(result2);
            result1.GetHashCode().Should().NotBe(result2.GetHashCode());
        }

        [Fact]
        public void Record_ToString_ContainsRelevantInformation()
        {
            // Arrange
            var result = new HealthCheckResult
            {
                IsHealthy = true,
                ResponseTime = TimeSpan.FromMilliseconds(150),
                Error = "Test error",
                CheckedAt = DateTime.UtcNow,
                Endpoint = "/test/endpoint",
            };

            // Act
            var toString = result.ToString();

            // Assert
            toString.Should().Contain("IsHealthy = True");
            toString.Should().Contain("ResponseTime = 00:00:00.1500000");
            toString.Should().Contain("Error = Test error");
            toString.Should().Contain("Endpoint = /test/endpoint");
        }

        [Theory]
        [InlineData(true, null, null)]
        [InlineData(false, "Error message", null)]
        [InlineData(true, null, "/api/health")]
        [InlineData(false, "Connection failed", "/api/health")]
        public void Constructor_WithVariousValues_InitializesCorrectly(
            bool isHealthy,
            string? error,
            string? endpoint
        )
        {
            // Arrange
            var responseTime = isHealthy ? TimeSpan.FromMilliseconds(100) : (TimeSpan?)null;
            var checkedAt = DateTime.UtcNow;

            // Act
            var result = new HealthCheckResult
            {
                IsHealthy = isHealthy,
                ResponseTime = responseTime,
                Error = error,
                CheckedAt = checkedAt,
                Endpoint = endpoint,
            };

            // Assert
            result.IsHealthy.Should().Be(isHealthy);
            result.ResponseTime.Should().Be(responseTime);
            result.Error.Should().Be(error);
            result.CheckedAt.Should().Be(checkedAt);
            result.Endpoint.Should().Be(endpoint);
        }

        [Fact]
        public void Success_WithZeroResponseTime_CreatesHealthyResult()
        {
            // Arrange
            var responseTime = TimeSpan.Zero;

            // Act
            var result = HealthCheckResult.Success(responseTime);

            // Assert
            result.IsHealthy.Should().BeTrue();
            result.ResponseTime.Should().Be(TimeSpan.Zero);
            result.Error.Should().BeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Success_WithNegativeResponseTime_CreatesHealthyResult()
        {
            // Arrange
            var responseTime = TimeSpan.FromMilliseconds(-100);

            // Act
            var result = HealthCheckResult.Success(responseTime);

            // Assert
            result.IsHealthy.Should().BeTrue();
            result.ResponseTime.Should().Be(responseTime);
            result.Error.Should().BeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
