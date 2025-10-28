using FluentAssertions;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Tests;

namespace StableDiffusionNet.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты для дополнительных сервисов (Embeddings, LoRA, Upscaler, PngInfo, Extra)
    /// </summary>
    [Trait("Category", TestCategories.Integration)]
    public sealed class AdditionalServicesIntegrationTests : IntegrationTestBase
    {
        #region Embeddings Tests

        [Fact]
        [Trait("Category", TestCategories.Smoke)]
        public async Task GetEmbeddingsAsync_ReturnsEmbeddingsDictionary()
        {
            // Act
            var embeddings = await Client.Embeddings.GetEmbeddingsAsync();

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
            var act = async () => await Client.Embeddings.RefreshEmbeddingsAsync();

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
            var loras = await Client.Loras.GetLorasAsync();

            // Assert
            loras.Should().NotBeNull();
            // Может быть пустым, если нет доступных LoRA моделей
            loras.Should().BeAssignableTo<IReadOnlyList<StableDiffusionNet.Models.Lora>>();
        }

        [Fact]
        public async Task RefreshLorasAsync_CompletesSuccessfully()
        {
            // Act
            var act = async () => await Client.Loras.RefreshLorasAsync();

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
            var upscalers = await Client.Upscalers.GetUpscalersAsync();

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
            var modes = await Client.Upscalers.GetLatentUpscaleModesAsync();

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

            var generateResponse = await Client.TextToImage.GenerateAsync(generateRequest);
            generateResponse.Images.Should().NotBeNull();
            generateResponse.Images.Should().NotBeEmpty();

            var pngInfoRequest = new PngInfoRequest { Image = generateResponse.Images![0] };

            // Act
            var pngInfo = await Client.PngInfo.GetPngInfoAsync(pngInfoRequest);

            // Assert
            pngInfo.Should().NotBeNull();
            pngInfo.Info.Should().NotBeNullOrEmpty();
            pngInfo.Info.Should().Contain("test image for metadata extraction");
        }

        [Fact]
        public async Task GetPngInfoAsync_WithInvalidImage_ThrowsOrReturnsEmpty()
        {
            // Arrange
            var request = new PngInfoRequest { Image = "invalid_base64_data" };

            // Act & Assert
            // В зависимости от реализации API, может выбросить исключение или вернуть пустой результат
            var act = async () => await Client.PngInfo.GetPngInfoAsync(request);

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

            var request = new ExtraSingleImageRequest
            {
                Image = simpleImage,
                UpscalingResize = 2, // Увеличиваем в 2 раза
                ResizeMode = 0,
            };

            // Act
            var response = await Client.Extra.ProcessSingleImageAsync(request);

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
            var upscalers = await Client.Upscalers.GetUpscalersAsync();
            upscalers.Should().NotBeEmpty("No upscalers available for testing");

            var firstUpscaler = upscalers[0].Name;

            var simpleImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var request = new ExtraSingleImageRequest
            {
                Image = simpleImage,
                UpscalingResize = 2,
                Upscaler1 = firstUpscaler,
                ResizeMode = 0,
            };

            // Act
            var response = await Client.Extra.ProcessSingleImageAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Image.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", TestCategories.LongRunning)]
        public async Task ExtraService_WithFaceRestoration_ProcessesImage()
        {
            // Arrange
            var simpleImage =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var request = new ExtraSingleImageRequest
            {
                Image = simpleImage,
                UpscalingResize = 2,
                ResizeMode = 0,
                GfpganVisibility = 0.5, // Включаем face restoration
                CodeformerVisibility = 0.5,
                CodeformerWeight = 0.5,
            };

            // Act
            var response = await Client.Extra.ProcessSingleImageAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Image.Should().NotBeNullOrEmpty();
        }

        #endregion
    }
}
