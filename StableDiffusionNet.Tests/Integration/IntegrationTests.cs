using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.DependencyInjection.Extensions;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Models.Requests;

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
        public async Task GetSchedulersAsync_ReturnsListOfSchedulers()
        {
            // Act
            var schedulers = await _client.Schedulers.GetSchedulersAsync();

            // Assert
            schedulers.Should().NotBeNull();
            schedulers.Should().NotBeEmpty();
            schedulers
                .Should()
                .Contain(s =>
                    s.Name.Contains("automatic", StringComparison.OrdinalIgnoreCase)
                    || s.Name.Contains("karras", StringComparison.OrdinalIgnoreCase)
                    || s.Name.Contains("uniform", StringComparison.OrdinalIgnoreCase)
                );
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

        #region Embeddings Tests

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetEmbeddingsAsync_ReturnsEmbeddingsDictionary()
        {
            // Act
            var embeddings = await _client.Embeddings.GetEmbeddingsAsync();

            // Assert
            embeddings.Should().NotBeNull();
            // Может быть пустым, если нет загруженных embeddings
            embeddings
                .Should()
                .BeAssignableTo<IReadOnlyDictionary<string, StableDiffusionNet.Models.Embedding>>();
        }

        [Fact]
        public async Task RefreshEmbeddingsAsync_CompletesSuccessfully()
        {
            // Act
            var act = async () => await _client.Embeddings.RefreshEmbeddingsAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region LoRA Tests

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetLorasAsync_ReturnsLoraList()
        {
            // Act
            var loras = await _client.Loras.GetLorasAsync();

            // Assert
            loras.Should().NotBeNull();
            // Может быть пустым, если нет доступных LoRA моделей
            loras.Should().BeAssignableTo<IReadOnlyList<StableDiffusionNet.Models.Lora>>();
        }

        [Fact]
        public async Task RefreshLorasAsync_CompletesSuccessfully()
        {
            // Act
            var act = async () => await _client.Loras.RefreshLorasAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region Upscaler Tests

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetUpscalersAsync_ReturnsUpscalerList()
        {
            // Act
            var upscalers = await _client.Upscalers.GetUpscalersAsync();

            // Assert
            upscalers.Should().NotBeNull();
            upscalers.Should().NotBeEmpty();
            upscalers.Should().Contain(u => !string.IsNullOrEmpty(u.Name));
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetLatentUpscaleModesAsync_ReturnsModesList()
        {
            // Act
            var modes = await _client.Upscalers.GetLatentUpscaleModesAsync();

            // Assert
            modes.Should().NotBeNull();
            modes.Should().NotBeEmpty();
            modes.Should().Contain(m => !string.IsNullOrEmpty(m));
        }

        #endregion

        #region PngInfo Tests

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task GetPngInfoAsync_WithGeneratedImage_ExtractsMetadata()
        {
            // Arrange - сначала генерируем изображение
            var generateRequest = new TextToImageRequest
            {
                Prompt = "test image for metadata extraction",
                Steps = 10,
                Width = 256,
                Height = 256,
            };

            var generateResponse = await _client.TextToImage.GenerateAsync(generateRequest);
            generateResponse.Images.Should().NotBeEmpty();

            var pngInfoRequest = new StableDiffusionNet.Models.Requests.PngInfoRequest
            {
                Image = generateResponse.Images[0],
            };

            // Act
            var pngInfo = await _client.PngInfo.GetPngInfoAsync(pngInfoRequest);

            // Assert
            pngInfo.Should().NotBeNull();
            pngInfo.Info.Should().NotBeNullOrEmpty();
            pngInfo.Info.Should().Contain("test image for metadata extraction");
        }

        [Fact]
        public async Task GetPngInfoAsync_WithInvalidImage_ThrowsOrReturnsEmpty()
        {
            // Arrange
            var request = new StableDiffusionNet.Models.Requests.PngInfoRequest
            {
                Image = "invalid_base64_data",
            };

            // Act & Assert
            // В зависимости от реализации API, может выбросить исключение или вернуть пустой результат
            var act = async () => await _client.PngInfo.GetPngInfoAsync(request);

            // Проверяем, что либо выбросится исключение, либо вернется пустой Info
            try
            {
                var result = await act.Invoke();
                result.Info.Should().BeNullOrEmpty();
            }
            catch
            {
                // Ожидаемое поведение - исключение при невалидных данных
                await act.Should().ThrowAsync<Exception>();
            }
        }

        #endregion

        #region Extra Tests

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task ProcessSingleImageAsync_WithUpscaling_ReturnsProcessedImage()
        {
            // Arrange - используем простое тестовое изображение
            var simpleImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var request = new StableDiffusionNet.Models.Requests.ExtraSingleImageRequest
            {
                Image = simpleImage,
                UpscalingResize = 2, // Увеличиваем в 2 раза
                ResizeMode = 0,
            };

            // Act
            var response = await _client.Extra.ProcessSingleImageAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Image.Should().NotBeNullOrEmpty();
            // Для изображения 1x1 результат может быть того же размера или немного больше
            response.Image!.Length.Should().BeGreaterOrEqualTo(simpleImage.Length);
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task ProcessSingleImageAsync_WithUpscalerSpecified_UsesSpecificUpscaler()
        {
            // Arrange
            var upscalers = await _client.Upscalers.GetUpscalersAsync();
            upscalers.Should().NotBeEmpty("No upscalers available for testing");

            var firstUpscaler = upscalers[0].Name;

            var simpleImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var request = new StableDiffusionNet.Models.Requests.ExtraSingleImageRequest
            {
                Image = simpleImage,
                UpscalingResize = 2,
                Upscaler1 = firstUpscaler,
                ResizeMode = 0,
            };

            // Act
            var response = await _client.Extra.ProcessSingleImageAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Image.Should().NotBeNullOrEmpty();
        }

        #endregion

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
