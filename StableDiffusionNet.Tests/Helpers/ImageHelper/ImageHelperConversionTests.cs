using FluentAssertions;
using StableDiffusionNet.Helpers;

namespace StableDiffusionNet.Tests.Helpers.ImageHelper
{
    /// <summary>
    /// Тесты для синхронных методов конверсии ImageHelper
    /// </summary>
    public sealed class ImageHelperConversionTests
    {
        private readonly StableDiffusionNet.Helpers.ImageHelper _imageHelper;

        public ImageHelperConversionTests()
        {
            _imageHelper = new StableDiffusionNet.Helpers.ImageHelper();
        }

        [Fact]
        public void BytesToBase64_ValidBytes_ReturnsBase64String()
        {
            // Arrange
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };

            // Act
            var result = _imageHelper.BytesToBase64(bytes);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("data:image/png;base64,");
        }

        [Fact]
        public void BytesToBase64_CustomMimeType_ReturnsCorrectMimeType()
        {
            // Arrange
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var mimeType = "image/jpeg";

            // Act
            var result = _imageHelper.BytesToBase64(bytes, mimeType);

            // Assert
            result.Should().StartWith($"data:{mimeType};base64,");
        }

        [Fact]
        public void BytesToBase64_NullBytes_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _imageHelper.BytesToBase64(null!);
            act.Should().Throw<ArgumentException>().WithParameterName("imageBytes");
        }

        [Fact]
        public void BytesToBase64_EmptyBytes_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _imageHelper.BytesToBase64(Array.Empty<byte>());
            act.Should().Throw<ArgumentException>().WithParameterName("imageBytes");
        }

        [Fact]
        public void ExtractBase64Data_WithPrefix_ReturnsDataOnly()
        {
            // Arrange
            var base64 = "data:image/png;base64,ABCD1234";

            // Act
            var result = _imageHelper.ExtractBase64Data(base64);

            // Assert
            result.Should().Be("ABCD1234");
        }

        [Fact]
        public void ExtractBase64Data_WithoutPrefix_ReturnsSameString()
        {
            // Arrange
            var base64 = "ABCD1234";

            // Act
            var result = _imageHelper.ExtractBase64Data(base64);

            // Assert
            result.Should().Be("ABCD1234");
        }

        [Fact]
        public void ExtractBase64Data_EmptyString_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _imageHelper.ExtractBase64Data(string.Empty);
            act.Should().Throw<ArgumentException>().WithParameterName("base64String");
        }
    }
}
