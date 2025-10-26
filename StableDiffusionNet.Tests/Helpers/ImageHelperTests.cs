using FluentAssertions;
using StableDiffusionNet.Helpers;

namespace StableDiffusionNet.Tests.Helpers
{
    /// <summary>
    /// Тесты для ImageHelper
    /// </summary>
    public class ImageHelperTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testImagePath;
        private readonly ImageHelper _imageHelper;

        public ImageHelperTests()
        {
            _imageHelper = new ImageHelper();
            _testDirectory = Path.Combine(Path.GetTempPath(), "StableDiffusionNet.Tests");
            Directory.CreateDirectory(_testDirectory);

            // Создаем тестовое изображение (1x1 PNG)
            _testImagePath = Path.Combine(_testDirectory, "test.png");
            var pngBytes = new byte[]
            {
                0x89,
                0x50,
                0x4E,
                0x47,
                0x0D,
                0x0A,
                0x1A,
                0x0A, // PNG signature
                0x00,
                0x00,
                0x00,
                0x0D,
                0x49,
                0x48,
                0x44,
                0x52, // IHDR chunk
                0x00,
                0x00,
                0x00,
                0x01,
                0x00,
                0x00,
                0x00,
                0x01, // 1x1 dimensions
                0x08,
                0x02,
                0x00,
                0x00,
                0x00,
                0x90,
                0x77,
                0x53,
                0xDE,
                0x00,
                0x00,
                0x00,
                0x0C,
                0x49,
                0x44,
                0x41,
                0x54,
                0x08,
                0xD7,
                0x63,
                0xF8,
                0xCF,
                0xC0,
                0x00,
                0x00,
                0x03,
                0x01,
                0x01,
                0x00,
                0x18,
                0xDD,
                0x8D,
                0xB4,
                0x00,
                0x00,
                0x00,
                0x00,
                0x49,
                0x45,
                0x4E,
                0x44,
                0xAE,
                0x42,
                0x60,
                0x82,
            };
            File.WriteAllBytes(_testImagePath, pngBytes);
        }

        [Fact]
        public void ImageToBase64_ValidPngFile_ReturnsBase64String()
        {
            // Act
            var result = _imageHelper.ImageToBase64(_testImagePath);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("data:image/png;base64,");
            result.Should().Contain(",");
        }

        [Fact]
        public void ImageToBase64_ValidJpegFile_ReturnsCorrectMimeType()
        {
            // Arrange
            var jpegPath = Path.Combine(_testDirectory, "test.jpg");
            File.WriteAllBytes(jpegPath, new byte[] { 0xFF, 0xD8, 0xFF });

            // Act
            var result = _imageHelper.ImageToBase64(jpegPath);

            // Assert
            result.Should().StartWith("data:image/jpeg;base64,");
        }

        [Fact]
        public void ImageToBase64_EmptyPath_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _imageHelper.ImageToBase64(string.Empty);
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("filePath")
                .WithMessage("*cannot be empty*");
        }

        [Fact]
        public void ImageToBase64_NullPath_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _imageHelper.ImageToBase64(null!);
            act.Should().Throw<ArgumentException>().WithParameterName("filePath");
        }

        [Fact]
        public void ImageToBase64_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.png");

            // Act & Assert
            var act = () => _imageHelper.ImageToBase64(nonExistentPath);
            act.Should().Throw<FileNotFoundException>();
        }

        [Theory]
        [InlineData("test.png", "image/png")]
        [InlineData("test.jpg", "image/jpeg")]
        [InlineData("test.jpeg", "image/jpeg")]
        [InlineData("test.gif", "image/gif")]
        [InlineData("test.webp", "image/webp")]
        [InlineData("test.bmp", "image/bmp")]
        [InlineData("test.unknown", "image/png")] // default
        public void ImageToBase64_DifferentExtensions_ReturnsCorrectMimeType(
            string filename,
            string expectedMime
        )
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, filename);
            File.WriteAllBytes(filePath, new byte[] { 0x01, 0x02, 0x03 });

            // Act
            var result = _imageHelper.ImageToBase64(filePath);

            // Assert
            result.Should().StartWith($"data:{expectedMime};base64,");
        }

        [Fact]
        public void Base64ToImage_ValidBase64WithPrefix_CreatesFile()
        {
            // Arrange
            var base64 = "data:image/png;base64,iVBORw0KGgo=";
            var outputPath = Path.Combine(_testDirectory, "output.png");

            // Act
            _imageHelper.Base64ToImage(base64, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
        }

        [Fact]
        public void Base64ToImage_ValidBase64WithoutPrefix_CreatesFile()
        {
            // Arrange
            var base64 = "iVBORw0KGgo=";
            var outputPath = Path.Combine(_testDirectory, "output2.png");

            // Act
            _imageHelper.Base64ToImage(base64, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
        }

        [Fact]
        public void Base64ToImage_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var base64 = "iVBORw0KGgo=";
            var subdirectory = Path.Combine(_testDirectory, "subdir");
            var outputPath = Path.Combine(subdirectory, "output.png");

            // Act
            _imageHelper.Base64ToImage(base64, outputPath);

            // Assert
            Directory.Exists(subdirectory).Should().BeTrue();
            File.Exists(outputPath).Should().BeTrue();
        }

        [Fact]
        public void Base64ToImage_EmptyBase64_ThrowsArgumentException()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "output.png");

            // Act & Assert
            var act = () => _imageHelper.Base64ToImage(string.Empty, outputPath);
            act.Should().Throw<ArgumentException>().WithParameterName("base64String");
        }

        [Fact]
        public void Base64ToImage_EmptyOutputPath_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _imageHelper.Base64ToImage("base64data", string.Empty);
            act.Should().Throw<ArgumentException>().WithParameterName("outputPath");
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

        [Fact]
        public void ImageToBase64_AndBack_PreservesData()
        {
            // Arrange
            var originalBytes = File.ReadAllBytes(_testImagePath);

            // Act
            var base64 = _imageHelper.ImageToBase64(_testImagePath);
            var outputPath = Path.Combine(_testDirectory, "roundtrip.png");
            _imageHelper.Base64ToImage(base64, outputPath);
            var resultBytes = File.ReadAllBytes(outputPath);

            // Assert
            resultBytes.Should().Equal(originalBytes);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
    }
}
