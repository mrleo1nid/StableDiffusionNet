using FluentAssertions;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Tests;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для TextToImage сервиса
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public sealed class TextToImageIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task TextToImageGenerateAsync_WithBasicPrompt_GeneratesImage()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "a beautiful mountain landscape, highly detailed",
                NegativePrompt = "blurry, low quality",
                Steps = 20,
                Width = 512,
                Height = 512,
                CfgScale = 7.0,
                SamplerName = "Euler a",
            };

            // Act
            var response = await Client.TextToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images![0].Should().NotBeNullOrEmpty();
            // Image should be base64 encoded or be raw base64 (length > 1000)
            var hasPrefix = response.Images[0].Contains("base64");
            var isLong = response.Images[0].Length > 1000;
            (hasPrefix || isLong).Should().BeTrue("Image should be valid base64");
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task TextToImageGenerateAsync_WithBatchSize_GeneratesMultipleImages()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "a simple test image",
                NegativePrompt = "blurry",
                Steps = 10,
                Width = 256,
                Height = 256,
                BatchSize = 2, // Генерируем 2 изображения
                CfgScale = 7.0,
                SamplerName = "Euler a",
            };

            // Act
            var response = await Client.TextToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeNull();
            response.Images.Should().HaveCount(2);
            response.Images!.All(img => !string.IsNullOrEmpty(img)).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task TextToImageGenerateAsync_WithDifferentSizes_GeneratesCorrectSizes()
        {
            // Arrange
            var sizes = new[] { (256, 256), (512, 512), (768, 768) };
            var results = new List<(int width, int height, string image)>();

            foreach (var (width, height) in sizes)
            {
                var request = new TextToImageRequest
                {
                    Prompt = "a simple test image",
                    Steps = 5, // Быстрая генерация для теста
                    Width = width,
                    Height = height,
                    BatchSize = 1,
                };

                // Act
                var response = await Client.TextToImage.GenerateAsync(request);

                // Assert
                response.Should().NotBeNull();
                response.Images.Should().NotBeEmpty();
                results.Add((width, height, response.Images![0]));
            }

            // Проверяем, что все изображения были сгенерированы
            results.Should().HaveCount(3);
            results.All(r => !string.IsNullOrEmpty(r.image)).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task TextToImageGenerateAsync_WithLoRA_GeneratesImageWithLoRA()
        {
            // Arrange
            var loras = await Client.Loras.GetLorasAsync();

            var request = new TextToImageRequest
            {
                Prompt = "a beautiful portrait",
                Steps = 10,
                Width = 512,
                Height = 512,
                BatchSize = 1,
            };

            // Если есть доступные LoRA модели, добавляем их в промпт
            if (loras.Any())
            {
                var firstLora = loras[0];
                // Добавляем LoRA в промпт в формате <lora:name:weight>
                request.Prompt = $"<lora:{firstLora.Name}:1.0>, {request.Prompt}";
            }

            // Act
            var response = await Client.TextToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images![0].Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task TextToImageGenerateAsync_WithEmbeddings_GeneratesImageWithEmbeddings()
        {
            // Arrange
            var embeddings = await Client.Embeddings.GetEmbeddingsAsync();

            var request = new TextToImageRequest
            {
                Prompt = "a beautiful landscape",
                Steps = 10,
                Width = 512,
                Height = 512,
                BatchSize = 1,
            };

            // Если есть доступные embeddings, добавляем их в промпт
            if (embeddings.Any())
            {
                var firstEmbedding = embeddings.First();
                request.Prompt = $"{firstEmbedding.Key}, {request.Prompt}";
            }

            // Act
            var response = await Client.TextToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images![0].Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task TextToImageGenerateAsync_WithInvalidParameters_ThrowsException()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "", // Пустой промпт должен вызвать ошибку
                Steps = 0, // Неверное количество шагов
                Width = -1, // Неверная ширина
                Height = -1, // Неверная высота
            };

            // Act & Assert
            var act = async () => await Client.TextToImage.GenerateAsync(request);
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
