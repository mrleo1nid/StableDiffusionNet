using FluentAssertions;
using StableDiffusionNet.Infrastructure;

namespace StableDiffusionNet.Tests.Infrastructure
{
    /// <summary>
    /// Тесты для JsonSanitizer
    /// </summary>
    public class JsonSanitizerTests
    {
        [Fact]
        public void SanitizeForLogging_ShortJson_ReturnsUnmodified()
        {
            // Arrange
            var shortJson = "{\"test\":\"value\"}";

            // Act
            var result = JsonSanitizer.SanitizeForLogging(shortJson);

            // Assert
            result.Should().Be(shortJson);
        }

        [Fact]
        public void SanitizeForLogging_LongJsonWithImageData_ReturnsSummary()
        {
            // Arrange
            var longBase64 = new string('A', 1000);
            var jsonWithImage = $"{{\"data:image/png;base64\":\"{longBase64}\"}}";

            // Act
            var result = JsonSanitizer.SanitizeForLogging(jsonWithImage);

            // Assert
            result.Should().Contain("[Request with image data, length:");
            result.Should().Contain("chars]");
            result.Should().NotContain(longBase64);
        }

        [Fact]
        public void SanitizeForLogging_LongJsonWithInitImages_ReturnsSummary()
        {
            // Arrange
            var longBase64 = new string('A', 1000);
            var jsonWithInitImages = $"{{\"init_images\":[\"{longBase64}\"]}}";

            // Act
            var result = JsonSanitizer.SanitizeForLogging(jsonWithInitImages);

            // Assert
            result.Should().Contain("[Request with image data, length:");
            result.Should().Contain("chars]");
            result.Should().NotContain(longBase64);
        }

        [Fact]
        public void SanitizeForLogging_LongJsonWithMask_ReturnsSummary()
        {
            // Arrange
            var longBase64 = new string('A', 1000);
            var jsonWithMask = $"{{\"mask\":\"{longBase64}\"}}";

            // Act
            var result = JsonSanitizer.SanitizeForLogging(jsonWithMask);

            // Assert
            result.Should().Contain("[Request with image data, length:");
            result.Should().Contain("chars]");
            result.Should().NotContain(longBase64);
        }

        [Fact]
        public void SanitizeForLogging_LongJsonWithoutImageData_ReturnsTruncated()
        {
            // Arrange
            var longJson = new string('a', 600);

            // Act
            var result = JsonSanitizer.SanitizeForLogging(longJson);

            // Assert
            result.Should().Contain("... [truncated, total length: 600 chars]");
            result.Length.Should().BeLessThan(600);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(250)]
        [InlineData(500)]
        public void SanitizeForLogging_JsonAtOrBelowMaxLength_ReturnsUnmodified(int length)
        {
            // Arrange
            var json = new string('x', length);

            // Act
            var result = JsonSanitizer.SanitizeForLogging(json);

            // Assert
            result.Should().Be(json);
        }

        [Fact]
        public void SanitizeForLogging_ExactlyMaxLength_ReturnsUnmodified()
        {
            // Arrange - MaxLength is 500
            var json = new string('x', 500);

            // Act
            var result = JsonSanitizer.SanitizeForLogging(json);

            // Assert
            result.Should().Be(json);
        }

        [Fact]
        public void SanitizeForLogging_JustOverMaxLength_ReturnsTruncated()
        {
            // Arrange - MaxLength is 500
            var json = new string('x', 501);

            // Act
            var result = JsonSanitizer.SanitizeForLogging(json);

            // Assert
            result.Should().Contain("... [truncated, total length: 501 chars]");
        }

        [Fact]
        public void SanitizeForLogging_EmptyString_ReturnsEmpty()
        {
            // Arrange
            var json = string.Empty;

            // Act
            var result = JsonSanitizer.SanitizeForLogging(json);

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void SanitizeForLogging_VeryLongJsonWithMultipleImageFields_ReturnsSummary()
        {
            // Arrange
            var longBase64 = new string('A', 2000);
            var jsonWithMultipleImages =
                $"{{\"data:image/png;base64\":\"{longBase64}\",\"init_images\":[\"{longBase64}\"],\"mask\":\"{longBase64}\"}}";

            // Act
            var result = JsonSanitizer.SanitizeForLogging(jsonWithMultipleImages);

            // Assert
            result.Should().Contain("[Request with image data, length:");
            result.Should().NotContain(longBase64);
        }
    }
}
