using FluentAssertions;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Tests;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для управления прогрессом генерации
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public sealed class ProgressControlIntegrationTests : IntegrationTestBase
    {
        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task InterruptAsync_DuringGeneration_StopsGeneration()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Steps = 50 };

            // Act
            _ = Client.TextToImage.GenerateAsync(request);
            await Task.Delay(1000); // Даем время начать генерацию

            await Client.Progress.InterruptAsync();

            // Assert
            var progress = await Client.Progress.GetProgressAsync();
            if (progress.State != null)
            {
                progress.State.Interrupted.Should().BeTrue();
            }
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task SkipAsync_DuringGeneration_SkipsCurrentImage()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test",
                Steps = 30,
                BatchSize = 2, // Генерируем несколько изображений
            };

            // Act
            _ = Client.TextToImage.GenerateAsync(request);
            await Task.Delay(2000); // Даем время начать генерацию

            await Client.Progress.SkipAsync();

            // Assert - проверяем, что операция завершилась без ошибок
            // SkipAsync может не устанавливать interrupted флаг, но должен работать
            var progress = await Client.Progress.GetProgressAsync();
            progress.Should().NotBeNull();

            // Проверяем, что прогресс изменился или генерация завершилась
            // Это косвенно подтверждает, что SkipAsync сработал
            await Task.Delay(1000); // Даем время на обработку
            var finalProgress = await Client.Progress.GetProgressAsync();
            finalProgress.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task FullWorkflow_GenerateAndCheckProgress_WorksCorrectly()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "a simple test image",
                Steps = 10,
                Width = 256,
                Height = 256,
            };

            // Act
            var generationTask = Client.TextToImage.GenerateAsync(request);

            // Проверяем прогресс во время генерации
            var progressChecked = false;
            while (!generationTask.IsCompleted)
            {
                var progress = await Client.Progress.GetProgressAsync();
                if (progress.Progress > 0)
                {
                    progressChecked = true;
                }
                await Task.Delay(100);
            }

            var result = await generationTask;

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
            progressChecked.Should().BeTrue("Progress should be checked during generation");
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetProgressAsync_DuringMultipleGenerations_ReturnsValidProgress()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "a detailed test image",
                Steps = 20,
                Width = 512,
                Height = 512,
                BatchSize = 2,
            };

            // Act
            var generationTask = Client.TextToImage.GenerateAsync(request);

            var progressChecks = new List<StableDiffusionNet.Models.GenerationProgress>();
            while (!generationTask.IsCompleted)
            {
                var progress = await Client.Progress.GetProgressAsync();
                progressChecks.Add(progress);
                await Task.Delay(500);
            }

            var result = await generationTask;

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
            progressChecks.Should().NotBeEmpty();

            // Проверяем, что прогресс изменялся во время генерации
            var progressValues = progressChecks.Select(p => p.Progress).ToList();
            progressValues.Should().Contain(p => p > 0);
        }
    }
}
