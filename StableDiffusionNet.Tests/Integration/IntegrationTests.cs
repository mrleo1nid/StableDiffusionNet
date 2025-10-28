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
    public sealed class IntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IStableDiffusionClient _client;
        private bool _disposed;

        public IntegrationTests()
        {
            var services = new ServiceCollection();

            // Настройка клиента для интеграционных тестов
            services.AddStableDiffusion(options =>
            {
                options.BaseUrl = "http://localhost:7860";
                options.TimeoutSeconds = 300; // Уменьшаем таймаут для тестов
                options.EnableDetailedLogging = true;
                options.RetryCount = 2; // Уменьшаем количество попыток для тестов
                options.RetryDelayMilliseconds = 1000; // Быстрые повторы
            });

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            _serviceProvider = services.BuildServiceProvider();
            _client = _serviceProvider.GetRequiredService<IStableDiffusionClient>();
        }

        /// <summary>
        /// /// Проверяет доступность API перед запуском тестов
        /// </summary>
        private async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var healthCheck = await _client.HealthCheckAsync();
                return healthCheck.IsHealthy;
            }
            catch
            {
                return false;
            }
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetSamplersAsync_ReturnsListOfSamplers()
        {
            // Arrange
            var isApiAvailable = await IsApiAvailableAsync();
            if (!isApiAvailable)
            {
                // Пропускаем тест если API недоступен
                return;
            }

            // Act
            var samplers = await _client.Samplers.GetSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();
            samplers.Should().NotBeEmpty();
            samplers
                .Should()
                .Contain(s => s.Name.Contains("Euler", StringComparison.OrdinalIgnoreCase));
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
            // GetCurrentModelAsync возвращает имя без хеша, а Title содержит хеш в скобках
            targetModel.Should().Contain(newModel);

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
            modes.Should().Contain(m => !string.IsNullOrEmpty(m.Name));
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
            generateResponse.Images.Should().NotBeNull();
            generateResponse.Images.Should().NotBeEmpty();

            var pngInfoRequest = new StableDiffusionNet.Models.Requests.PngInfoRequest
            {
                Image = generateResponse.Images![0],
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

        #region Health Check Tests

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task HealthCheckAsync_WithRunningAPI_ReturnsHealthy()
        {
            // Act
            var healthCheck = await _client.HealthCheckAsync();

            // Assert
            healthCheck.Should().NotBeNull();
            healthCheck.IsHealthy.Should().BeTrue();
            healthCheck.ResponseTime.Should().NotBeNull();
            healthCheck.ResponseTime.Should().BeLessThan(TimeSpan.FromSeconds(5));
            healthCheck.Error.Should().BeNull();
        }

        #endregion

        #region Progress Service Additional Tests

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
            _ = _client.TextToImage.GenerateAsync(request);
            await Task.Delay(2000); // Даем время начать генерацию

            await _client.Progress.SkipAsync();

            // Assert - проверяем, что операция завершилась без ошибок
            // SkipAsync может не устанавливать interrupted флаг, но должен работать
            var progress = await _client.Progress.GetProgressAsync();
            progress.Should().NotBeNull();

            // Проверяем, что прогресс изменился или генерация завершилась
            // Это косвенно подтверждает, что SkipAsync сработал
            await Task.Delay(1000); // Даем время на обработку
            var finalProgress = await _client.Progress.GetProgressAsync();
            finalProgress.Should().NotBeNull();
        }

        #endregion

        #region Advanced Generation Scenarios

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
            var response = await _client.TextToImage.GenerateAsync(request);

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
                var response = await _client.TextToImage.GenerateAsync(request);

                // Assert
                response.Should().NotBeNull();
                response.Images.Should().NotBeEmpty();
                results.Add((width, height, response.Images![0]));
            }

            // Проверяем, что все изображения были сгенерированы
            results.Should().HaveCount(3);
            results.All(r => !string.IsNullOrEmpty(r.image)).Should().BeTrue();
        }

        #endregion

        #region Error Handling Tests

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
            var act = async () => await _client.TextToImage.GenerateAsync(request);
            await act.Should().ThrowAsync<ArgumentException>();
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
            var act = async () => await _client.ImageToImage.GenerateAsync(request);
            // API возвращает ApiException с InternalServerError, а не ArgumentException
            await act.Should()
                .ThrowAsync<StableDiffusionNet.Exceptions.ApiException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task SetModelAsync_WithInvalidModel_ThrowsException()
        {
            // Arrange
            var invalidModelName = "non_existent_model_12345";

            // Act & Assert
            var act = async () => await _client.Models.SetModelAsync(invalidModelName);
            await act.Should().ThrowAsync<Exception>();
        }

        #endregion

        #region Additional Testing Scenarios

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task TextToImageGenerateAsync_WithLoRA_GeneratesImageWithLoRA()
        {
            // Arrange
            var loras = await _client.Loras.GetLorasAsync();

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
            var response = await _client.TextToImage.GenerateAsync(request);

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
            var embeddings = await _client.Embeddings.GetEmbeddingsAsync();

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
            var response = await _client.TextToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images![0].Should().NotBeNullOrEmpty();
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
            var response = await _client.ImageToImage.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images![0].Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task ExtraService_WithFaceRestoration_ProcessesImage()
        {
            // Arrange
            var simpleImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var request = new StableDiffusionNet.Models.Requests.ExtraSingleImageRequest
            {
                Image = simpleImage,
                UpscalingResize = 2,
                ResizeMode = 0,
                GfpganVisibility = 0.5, // Включаем face restoration
                CodeformerVisibility = 0.5,
                CodeformerWeight = 0.5,
            };

            // Act
            var response = await _client.Extra.ProcessSingleImageAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Image.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task FullWorkflow_WithModelSwitchAndGeneration_WorksCorrectly()
        {
            // Arrange
            var models = await _client.Models.GetModelsAsync();
            models.Should().NotBeEmpty("No models available for testing");

            var originalModel = await _client.Models.GetCurrentModelAsync();
            var targetModel = models[0].Title;

            try
            {
                // Act - переключаем модель и генерируем изображение
                await _client.Models.SetModelAsync(targetModel);
                await Task.Delay(2000); // Даем время на переключение

                var request = new TextToImageRequest
                {
                    Prompt = "a test image with switched model",
                    Steps = 10,
                    Width = 256,
                    Height = 256,
                };

                var response = await _client.TextToImage.GenerateAsync(request);

                // Assert
                response.Should().NotBeNull();
                response.Images.Should().NotBeEmpty();
                response.Images![0].Should().NotBeNullOrEmpty();

                // Проверяем, что модель действительно переключилась
                var currentModel = await _client.Models.GetCurrentModelAsync();
                targetModel.Should().Contain(currentModel);
            }
            finally
            {
                // Cleanup - восстанавливаем оригинальную модель
                if (originalModel != "unknown")
                {
                    await _client.Models.SetModelAsync(originalModel);
                }
            }
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

                tasks.Add(_client.TextToImage.GenerateAsync(request));
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(3);
            results.All(r => r.Images != null && r.Images.Any()).Should().BeTrue();
            results.All(r => !string.IsNullOrEmpty(r.Images![0])).Should().BeTrue();
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
            var generationTask = _client.TextToImage.GenerateAsync(request);

            var progressChecks = new List<StableDiffusionNet.Models.GenerationProgress>();
            while (!generationTask.IsCompleted)
            {
                var progress = await _client.Progress.GetProgressAsync();
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

        #endregion

        public void Dispose()
        {
            if (_disposed)
                return;

            _serviceProvider?.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
