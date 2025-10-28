using FluentAssertions;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Tests;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для продвинутых сценариев использования
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public sealed class AdvancedScenariosIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task SetOptionsAsync_WithValidOptions_UpdatesOptions()
        {
            // Arrange
            var originalOptions = await Client.Options.GetOptionsAsync();
            var currentValue = originalOptions.ClipStopAtLastLayers ?? 1.0;
            var newClipValue = Math.Abs(currentValue - 1.0) < 0.001 ? 2.0 : 1.0;

            var updatedOptions = new StableDiffusionNet.Models.WebUIOptions
            {
                ClipStopAtLastLayers = newClipValue,
            };

            // Act
            await Client.Options.SetOptionsAsync(updatedOptions);
            await Task.Delay(500); // Дать время на применение

            var resultOptions = await Client.Options.GetOptionsAsync();

            // Assert
            resultOptions.ClipStopAtLastLayers.Should().Be(newClipValue);

            // Cleanup - восстанавливаем оригинальные настройки
            await Client.Options.SetOptionsAsync(originalOptions);
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task ConcurrentGenerations_WithDifferentParameters_WorkCorrectly()
        {
            // Arrange
            var tasks = new List<Task<StableDiffusionNet.Models.Responses.TextToImageResponse>>();

            // Создаем несколько задач генерации с разными параметрами
            for (int i = 0; i < 3; i++)
            {
                var request = new TextToImageRequest
                {
                    Prompt = $"test image {i + 1}",
                    Steps = 5, // Быстрая генерация
                    Width = 256,
                    Height = 256,
                    CfgScale = 7.0 + i, // Разные CFG значения
                    BatchSize = 1,
                };

                tasks.Add(Client.TextToImage.GenerateAsync(request));
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(3);
            results.All(r => r.Images != null && r.Images.Any()).Should().BeTrue();
            results.All(r => !string.IsNullOrEmpty(r.Images![0])).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task FullWorkflow_WithModelSwitchAndGeneration_WorksCorrectly()
        {
            // Arrange
            var models = await Client.Models.GetModelsAsync();
            models.Should().NotBeEmpty("No models available for testing");

            var originalModel = await Client.Models.GetCurrentModelAsync();
            var targetModel = models[0].Title;

            try
            {
                // Act - переключаем модель и генерируем изображение
                await Client.Models.SetModelAsync(targetModel);
                await Task.Delay(2000); // Даем время на переключение

                var request = new TextToImageRequest
                {
                    Prompt = "a test image with switched model",
                    Steps = 10,
                    Width = 256,
                    Height = 256,
                };

                var response = await Client.TextToImage.GenerateAsync(request);

                // Assert
                response.Should().NotBeNull();
                response.Images.Should().NotBeEmpty();
                response.Images![0].Should().NotBeNullOrEmpty();

                // Проверяем, что модель действительно переключилась
                var currentModel = await Client.Models.GetCurrentModelAsync();
                currentModel.Should().Contain(targetModel);
            }
            finally
            {
                // Cleanup - восстанавливаем оригинальную модель
                if (originalModel != "unknown")
                {
                    await Client.Models.SetModelAsync(originalModel);
                }
            }
        }
    }
}
