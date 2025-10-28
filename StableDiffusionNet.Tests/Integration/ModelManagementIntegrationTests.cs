using FluentAssertions;
using StableDiffusionNet.Tests;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для управления моделями
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public sealed class ModelManagementIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task SetModelAsync_WithValidModel_ChangesModel()
        {
            // Arrange
            var models = await Client.Models.GetModelsAsync();
            models.Should().NotBeEmpty("No models available for testing");

            var targetModel = models[0].Title;
            var originalModel = await Client.Models.GetCurrentModelAsync();

            // Act
            await Client.Models.SetModelAsync(targetModel);
            await Task.Delay(2000); // Дать время на переключение модели

            var newModel = await Client.Models.GetCurrentModelAsync();

            // Assert
            // GetCurrentModelAsync возвращает имя без хеша, а Title содержит хеш в скобках
            targetModel.Should().Contain(newModel);

            // Cleanup - восстанавливаем оригинальную модель
            if (originalModel != "unknown")
            {
                await Client.Models.SetModelAsync(originalModel);
            }
        }

        [Fact]
        public async Task RefreshModelsAsync_CompletesSuccessfully()
        {
            // Act
            var act = async () => await Client.Models.RefreshModelsAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task SetModelAsync_WithInvalidModel_ThrowsException()
        {
            // Arrange
            var invalidModelName = "non_existent_model_12345";

            // Act & Assert
            var act = async () => await Client.Models.SetModelAsync(invalidModelName);
            await act.Should().ThrowAsync<Exception>();
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

                var request = new StableDiffusionNet.Models.Requests.TextToImageRequest
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
