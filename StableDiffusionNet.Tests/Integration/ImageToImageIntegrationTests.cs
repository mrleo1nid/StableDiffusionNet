using FluentAssertions;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Tests;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для ImageToImage сервиса
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public sealed class ImageToImageIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task ImageToImageGenerateAsync_WithValidImage_GeneratesImage()
        {
            // Arrange
            // Создаем простое base64 изображение для теста
            var simpleImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { simpleImage },
                Prompt = "a beautiful landscape",
                Steps = 10,
                DenoisingStrength = 0.5,
            };

            // Act
            var response = await Client.ImageToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task ImageToImageGenerateAsync_WithDifferentFormats_ProcessesCorrectly()
        {
            // Arrange - используем простое PNG изображение 1x1 пиксель
            var testImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { testImage },
                Prompt = "make it colorful",
                Steps = 5, // Быстрая генерация для теста
                DenoisingStrength = 0.5,
                Width = 256,
                Height = 256,
            };

            // Act
            var response = await Client.ImageToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images![0].Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task ImageToImageGenerateAsync_WithInvalidImage_ThrowsException()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "invalid_base64_data" },
                Prompt = "test",
                Steps = 10,
            };

            // Act & Assert
            var act = async () => await Client.ImageToImage.GenerateAsync(request);
            // API возвращает ApiException с InternalServerError, а не ArgumentException
            await act.Should()
                .ThrowAsync<StableDiffusionNet.Exceptions.ApiException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
