using FluentAssertions;
using StableDiffusionNet.Helpers;

namespace StableDiffusionNet.Tests.Helpers.ImageHelper
{
    /// <summary>
    /// Тесты для методов валидации ImageHelper
    /// </summary>
    public sealed class ImageHelperValidationTests
    {
        private readonly StableDiffusionNet.Helpers.ImageHelper _imageHelper;

        public ImageHelperValidationTests()
        {
            _imageHelper = new StableDiffusionNet.Helpers.ImageHelper();
        }

        [Fact]
        public void ValidateImageBase64_ValidPng_DoesNotThrow()
        {
            // Arrange
            var pngBytes = new byte[]
            {
                0x89,
                0x50,
                0x4E,
                0x47,
                0x0D,
                0x0A,
                0x1A,
                0x0A,
                0x00,
                0x00,
                0x00,
                0x00,
            };
            var base64 = Convert.ToBase64String(pngBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateImageBase64_ValidPngWithPrefix_DoesNotThrow()
        {
            // Arrange
            var pngBytes = new byte[]
            {
                0x89,
                0x50,
                0x4E,
                0x47,
                0x0D,
                0x0A,
                0x1A,
                0x0A,
                0x00,
                0x00,
                0x00,
                0x00,
            };
            var base64 = $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateImageBase64_ValidJpeg_DoesNotThrow()
        {
            // Arrange
            var jpegBytes = new byte[]
            {
                0xFF,
                0xD8,
                0xFF,
                0xE0,
                0x00,
                0x10,
                0x4A,
                0x46,
                0x49,
                0x46,
                0x00,
                0x01,
            };
            var base64 = Convert.ToBase64String(jpegBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateImageBase64_ValidGif_DoesNotThrow()
        {
            // Arrange
            var gifBytes = new byte[]
            {
                0x47,
                0x49,
                0x46,
                0x38,
                0x39,
                0x61,
                0x01,
                0x00,
                0x01,
                0x00,
                0x00,
                0x00,
            };
            var base64 = Convert.ToBase64String(gifBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateImageBase64_ValidWebp_DoesNotThrow()
        {
            // Arrange
            var webpBytes = new byte[]
            {
                0x52,
                0x49,
                0x46,
                0x46,
                0x00,
                0x00,
                0x00,
                0x00,
                0x57,
                0x45,
                0x42,
                0x50,
            };
            var base64 = Convert.ToBase64String(webpBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateImageBase64_ValidBmp_DoesNotThrow()
        {
            // Arrange
            var bmpBytes = new byte[]
            {
                0x42,
                0x4D,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            };
            var base64 = Convert.ToBase64String(bmpBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateImageBase64_InvalidFormat_ThrowsArgumentException()
        {
            // Arrange
            var invalidBytes = new byte[]
            {
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            };
            var base64 = Convert.ToBase64String(invalidBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("base64String")
                .WithMessage("*not contain a valid image*");
        }

        [Fact]
        public void ValidateImageBase64_EmptyString_ThrowsArgumentException()
        {
            // Act
            var act = () => _imageHelper.ValidateImageBase64(string.Empty);

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("base64String");
        }

        [Fact]
        public void ValidateImageBase64_NullString_ThrowsArgumentException()
        {
            // Act
            var act = () => _imageHelper.ValidateImageBase64(null!);

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("base64String");
        }

        [Fact]
        public void ValidateImageBase64_InvalidBase64_ThrowsArgumentException()
        {
            // Arrange
            var invalidBase64 = "this is not valid base64!@#$%";

            // Act
            var act = () => _imageHelper.ValidateImageBase64(invalidBase64);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("base64String")
                .WithMessage("*Invalid base64 string format*");
        }

        [Fact]
        public void ValidateImageBase64_TooShortData_ThrowsArgumentException()
        {
            // Arrange - менее 12 байт, недостаточно для определения формата
            var shortBytes = new byte[] { 0x89, 0x50, 0x4E };
            var base64 = Convert.ToBase64String(shortBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("base64String")
                .WithMessage("*not contain a valid image*");
        }

        [Fact]
        public void ValidateImageBase64_WebpTooShort_ThrowsArgumentException()
        {
            // Arrange - WebP файл короче минимальной длины (12 байт)
            // RIFF заголовок но без WEBP маркера
            var shortWebpBytes = new byte[]
            {
                0x52,
                0x49,
                0x46,
                0x46, // RIFF
                0x00,
                0x00,
                0x00,
                0x00, // размер
                0x57,
                0x45,
                0x42, // WEB (неполный WEBP)
            };
            var base64 = Convert.ToBase64String(shortWebpBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("base64String")
                .WithMessage("*not contain a valid image*");
        }

        [Fact]
        public void ValidateImageBase64_WebpInvalidMarker_ThrowsArgumentException()
        {
            // Arrange - WebP файл с правильной длиной но неправильным маркером
            var invalidWebpBytes = new byte[]
            {
                0x52,
                0x49,
                0x46,
                0x46, // RIFF
                0x00,
                0x00,
                0x00,
                0x00, // размер
                0x57,
                0x45,
                0x42,
                0x51, // WEBQ вместо WEBP
            };
            var base64 = Convert.ToBase64String(invalidWebpBytes);

            // Act
            var act = () => _imageHelper.ValidateImageBase64(base64);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("base64String")
                .WithMessage("*not contain a valid image*");
        }
    }
}
