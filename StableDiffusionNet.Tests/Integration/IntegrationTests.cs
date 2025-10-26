using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.DependencyInjection.Extensions;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Models.Requests;
using Xunit;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для StableDiffusionClient
    /// Требуют запущенного Stable Diffusion WebUI API
    /// Используйте фильтры категорий для запуска:
    /// - Category=Integration - все интеграционные тесты
    /// - Category=Smoke - быстрые проверки API
    /// - Category!=LongRunning - исключить долгие тесты
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public class IntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IStableDiffusionClient _client;

        public IntegrationTests()
        {
            var services = new ServiceCollection();

            // Настройка клиента для интеграционных тестов
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.TimeoutSeconds = 600;
                options.EnableDetailedLogging = true;
            });

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            _serviceProvider = services.BuildServiceProvider();
            _client = _serviceProvider.GetRequiredService<IStableDiffusionClient>();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task PingAsync_WithRunningApi_ReturnsTrue()
        {
            // Act
            var result = await _client.PingAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetSamplersAsync_ReturnsListOfSamplers()
        {
            // Act
            var samplers = await _client.Samplers.GetSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();
            samplers.Should().NotBeEmpty();
            samplers.Should().Contain(s => s.Contains("Euler"));
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetModelsAsync_ReturnsListOfModels()
        {
            // Act
            var models = await _client.Models.GetModelsAsync();

            // Assert
            models.Should().NotBeNull();
            models.Should().NotBeEmpty();
            models[0].Title.Should().NotBeNullOrEmpty();
            models[0].ModelName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetCurrentModelAsync_ReturnsModelName()
        {
            // Act
            var currentModel = await _client.Models.GetCurrentModelAsync();

            // Assert
            currentModel.Should().NotBeNullOrEmpty();
            currentModel.Should().NotBe("unknown");
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetOptionsAsync_ReturnsWebUIOptions()
        {
            // Act
            var options = await _client.Options.GetOptionsAsync();

            // Assert
            options.Should().NotBeNull();
            options.SdModelCheckpoint.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetProgressAsync_ReturnsProgress()
        {
            // Act
            var progress = await _client.Progress.GetProgressAsync();

            // Assert
            progress.Should().NotBeNull();
            progress.Progress.Should().BeInRange(0.0, 1.0);
        }

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
            var response = await _client.TextToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images[0].Should().NotBeNullOrEmpty();
            // Image should be base64 encoded or be raw base64 (length > 1000)
            var hasPrefix = response.Images[0].Contains("base64");
            var isLong = response.Images[0].Length > 1000;
            (hasPrefix || isLong).Should().BeTrue("Image should be valid base64");
        }

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
            var response = await _client.ImageToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task SetModelAsync_WithValidModel_ChangesModel()
        {
            // Arrange
            var models = await _client.Models.GetModelsAsync();
            models.Should().NotBeEmpty("No models available for testing");

            var targetModel = models[0].Title;
            var originalModel = await _client.Models.GetCurrentModelAsync();

            // Act
            await _client.Models.SetModelAsync(targetModel);
            await Task.Delay(2000); // Дать время на переключение модели

            var newModel = await _client.Models.GetCurrentModelAsync();

            // Assert
            newModel.Should().Contain(targetModel);

            // Cleanup - восстанавливаем оригинальную модель
            if (originalModel != "unknown")
            {
                await _client.Models.SetModelAsync(originalModel);
            }
        }

        [Fact]
        public async Task RefreshModelsAsync_CompletesSuccessfully()
        {
            // Act
            var act = async () => await _client.Models.RefreshModelsAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SetOptionsAsync_WithValidOptions_UpdatesOptions()
        {
            // Arrange
            var originalOptions = await _client.Options.GetOptionsAsync();
            var currentValue = originalOptions.ClipStopAtLastLayers ?? 1.0;
            var newClipValue = Math.Abs(currentValue - 1.0) < 0.001 ? 2.0 : 1.0;

            var updatedOptions = new StableDiffusionNet.Models.WebUIOptions
            {
                ClipStopAtLastLayers = newClipValue,
            };

            // Act
            await _client.Options.SetOptionsAsync(updatedOptions);
            await Task.Delay(500); // Дать время на применение

            var resultOptions = await _client.Options.GetOptionsAsync();

            // Assert
            resultOptions.ClipStopAtLastLayers.Should().Be(newClipValue);

            // Cleanup - восстанавливаем оригинальные настройки
            await _client.Options.SetOptionsAsync(originalOptions);
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task InterruptAsync_DuringGeneration_StopsGeneration()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test", Steps = 50 };

            // Act
            _ = _client.TextToImage.GenerateAsync(request);
            await Task.Delay(1000); // Даем время начать генерацию

            await _client.Progress.InterruptAsync();

            // Assert
            var progress = await _client.Progress.GetProgressAsync();
            if (progress.State != null)
            {
                progress.State.Interrupted.Should().BeTrue();
            }
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
            var generationTask = _client.TextToImage.GenerateAsync(request);

            // Проверяем прогресс во время генерации
            var progressChecked = false;
            while (!generationTask.IsCompleted)
            {
                var progress = await _client.Progress.GetProgressAsync();
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

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
