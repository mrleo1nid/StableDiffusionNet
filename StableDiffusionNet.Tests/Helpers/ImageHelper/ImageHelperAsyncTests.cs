using FluentAssertions;
using StableDiffusionNet.Helpers;

namespace StableDiffusionNet.Tests.Helpers.ImageHelper
{
    /// <summary>
    /// Тесты для асинхронных методов ImageHelper
    /// </summary>
    public sealed class ImageHelperAsyncTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testImagePath;
        private readonly StableDiffusionNet.Helpers.ImageHelper _imageHelper;
        private bool _disposed;

        public ImageHelperAsyncTests()
        {
            _imageHelper = new StableDiffusionNet.Helpers.ImageHelper();
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
        public async Task ImageToBase64Async_ValidPngFile_ReturnsBase64String()
        {
            // Act
            var result = await _imageHelper.ImageToBase64Async(_testImagePath);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("data:image/png;base64,");
            result.Should().Contain(",");
        }

        [Fact]
        public async Task ImageToBase64Async_ValidJpegFile_ReturnsCorrectMimeType()
        {
            // Arrange
            var jpegPath = Path.Combine(_testDirectory, "test_async.jpg");
            // JPEG должен иметь минимум 12 байт для проверки magic bytes
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
            await File.WriteAllBytesAsync(jpegPath, jpegBytes);

            // Act
            var result = await _imageHelper.ImageToBase64Async(jpegPath);

            // Assert
            result.Should().StartWith("data:image/jpeg;base64,");
        }

        [Fact]
        public async Task ImageToBase64Async_EmptyPath_ThrowsArgumentException()
        {
            // Act
            var act = async () => await _imageHelper.ImageToBase64Async(string.Empty);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*")
                .WithParameterName("filePath");
        }

        [Fact]
        public async Task ImageToBase64Async_NullPath_ThrowsArgumentException()
        {
            // Act
            var act = async () => await _imageHelper.ImageToBase64Async(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("filePath");
        }

        [Fact]
        public async Task ImageToBase64Async_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent_async.png");

            // Act
            var act = async () => await _imageHelper.ImageToBase64Async(nonExistentPath);

            // Assert
            await act.Should().ThrowAsync<FileNotFoundException>();
        }

        [Fact]
        public async Task ImageToBase64Async_FileSizeExceedsLimit_ThrowsArgumentException()
        {
            // Arrange
            var largePath = Path.Combine(_testDirectory, "large_async.png");
            // Создаем файл размером 51 МБ (больше лимита в 50 МБ)
            var largeData = new byte[51 * 1024 * 1024];
            await File.WriteAllBytesAsync(largePath, largeData);

            // Act
            var act = async () => await _imageHelper.ImageToBase64Async(largePath);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithParameterName("filePath")
                .WithMessage("*exceeds maximum allowed size*");
        }

        [Fact]
        public async Task ImageToBase64Async_InvalidImageFormat_ThrowsArgumentException()
        {
            // Arrange
            var invalidPath = Path.Combine(_testDirectory, "invalid.png");
            // Создаем файл с невалидными magic bytes
            await File.WriteAllBytesAsync(
                invalidPath,
                new byte[]
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
                }
            );

            // Act
            var act = async () => await _imageHelper.ImageToBase64Async(invalidPath);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithParameterName("filePath")
                .WithMessage("*not a valid image format*");
        }

        [Fact]
        public async Task ImageToBase64Async_RealJpegBytes_ReturnsJpegMimeType()
        {
            // Arrange
            var jpegPath = Path.Combine(_testDirectory, "real.jpg");
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
            await File.WriteAllBytesAsync(jpegPath, jpegBytes);

            // Act
            var result = await _imageHelper.ImageToBase64Async(jpegPath);

            // Assert
            result.Should().StartWith("data:image/jpeg;base64,");
        }

        [Fact]
        public async Task ImageToBase64Async_RealGifBytes_ReturnsGifMimeType()
        {
            // Arrange
            var gifPath = Path.Combine(_testDirectory, "real.gif");
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
            await File.WriteAllBytesAsync(gifPath, gifBytes);

            // Act
            var result = await _imageHelper.ImageToBase64Async(gifPath);

            // Assert
            result.Should().StartWith("data:image/gif;base64,");
        }

        [Fact]
        public async Task ImageToBase64Async_RealWebpBytes_ReturnsWebpMimeType()
        {
            // Arrange
            var webpPath = Path.Combine(_testDirectory, "real.webp");
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
            await File.WriteAllBytesAsync(webpPath, webpBytes);

            // Act
            var result = await _imageHelper.ImageToBase64Async(webpPath);

            // Assert
            result.Should().StartWith("data:image/webp;base64,");
        }

        [Fact]
        public async Task ImageToBase64Async_RealBmpBytes_ReturnsBmpMimeType()
        {
            // Arrange
            var bmpPath = Path.Combine(_testDirectory, "real.bmp");
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
            await File.WriteAllBytesAsync(bmpPath, bmpBytes);

            // Act
            var result = await _imageHelper.ImageToBase64Async(bmpPath);

            // Assert
            result.Should().StartWith("data:image/bmp;base64,");
        }

        [Fact]
        public async Task ImageToBase64Async_WithCancellationToken_CanBeCancelled()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var act = async () => await _imageHelper.ImageToBase64Async(_testImagePath, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task Base64ToImageAsync_ValidBase64WithPrefix_CreatesFile()
        {
            // Arrange
            var base64 = "data:image/png;base64,iVBORw0KGgo=";
            var outputPath = Path.Combine(_testDirectory, "output_async.png");

            // Act
            await _imageHelper.Base64ToImageAsync(base64, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
        }

        [Fact]
        public async Task Base64ToImageAsync_ValidBase64WithoutPrefix_CreatesFile()
        {
            // Arrange
            var base64 = "iVBORw0KGgo=";
            var outputPath = Path.Combine(_testDirectory, "output_async2.png");

            // Act
            await _imageHelper.Base64ToImageAsync(base64, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
        }

        [Fact]
        public async Task Base64ToImageAsync_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var base64 = "iVBORw0KGgo=";
            var subdirectory = Path.Combine(_testDirectory, "subdir_async");
            var outputPath = Path.Combine(subdirectory, "output.png");

            // Act
            await _imageHelper.Base64ToImageAsync(base64, outputPath);

            // Assert
            Directory.Exists(subdirectory).Should().BeTrue();
            File.Exists(outputPath).Should().BeTrue();
        }

        [Fact]
        public async Task Base64ToImageAsync_EmptyBase64_ThrowsArgumentException()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "output_async.png");

            // Act
            var act = async () => await _imageHelper.Base64ToImageAsync(string.Empty, outputPath);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("base64String");
        }

        [Fact]
        public async Task Base64ToImageAsync_EmptyOutputPath_ThrowsArgumentException()
        {
            // Act
            var act = async () => await _imageHelper.Base64ToImageAsync("base64data", string.Empty);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("outputPath");
        }

        [Fact]
        public async Task Base64ToImageAsync_InvalidBase64_ThrowsArgumentException()
        {
            // Arrange
            var invalidBase64 = "this is not valid base64!@#$%";
            var outputPath = Path.Combine(_testDirectory, "output_invalid.png");

            // Act
            var act = async () => await _imageHelper.Base64ToImageAsync(invalidBase64, outputPath);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid base64 string format*");
        }

        [Fact]
        public async Task Base64ToImageAsync_WithCancellationToken_CanBeCancelled()
        {
            // Arrange
            var base64 = "iVBORw0KGgo=";
            var outputPath = Path.Combine(_testDirectory, "output_cancelled.png");
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var act = async () =>
                await _imageHelper.Base64ToImageAsync(base64, outputPath, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ImageToBase64Async_AndBackAsync_PreservesData()
        {
            // Arrange
            var originalBytes = await File.ReadAllBytesAsync(_testImagePath);

            // Act
            var base64 = await _imageHelper.ImageToBase64Async(_testImagePath);
            var outputPath = Path.Combine(_testDirectory, "roundtrip_async.png");
            await _imageHelper.Base64ToImageAsync(base64, outputPath);
            var resultBytes = await File.ReadAllBytesAsync(outputPath);

            // Assert
            resultBytes.Should().Equal(originalBytes);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
