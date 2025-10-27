using FluentAssertions;
using Moq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Tests
{
    /// <summary>
    /// Тесты для StableDiffusionClient
    /// </summary>
    public class StableDiffusionClientTests
    {
        private readonly Mock<ITextToImageService> _textToImageMock;
        private readonly Mock<IImageToImageService> _imageToImageMock;
        private readonly Mock<IModelService> _modelServiceMock;
        private readonly Mock<IProgressService> _progressServiceMock;
        private readonly Mock<IOptionsService> _optionsServiceMock;
        private readonly Mock<ISamplerService> _samplerServiceMock;
        private readonly Mock<ISchedulerService> _schedulerServiceMock;
        private readonly Mock<IUpscalerService> _upscalerServiceMock;
        private readonly Mock<IPngInfoService> _pngInfoServiceMock;
        private readonly Mock<IExtraService> _extraServiceMock;
        private readonly Mock<IEmbeddingService> _embeddingServiceMock;
        private readonly Mock<ILoraService> _loraServiceMock;
        private readonly Mock<IHttpClientWrapper> _httpClientWrapperMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;

        public StableDiffusionClientTests()
        {
            _textToImageMock = new Mock<ITextToImageService>();
            _imageToImageMock = new Mock<IImageToImageService>();
            _modelServiceMock = new Mock<IModelService>();
            _progressServiceMock = new Mock<IProgressService>();
            _optionsServiceMock = new Mock<IOptionsService>();
            _samplerServiceMock = new Mock<ISamplerService>();
            _schedulerServiceMock = new Mock<ISchedulerService>();
            _upscalerServiceMock = new Mock<IUpscalerService>();
            _pngInfoServiceMock = new Mock<IPngInfoService>();
            _extraServiceMock = new Mock<IExtraService>();
            _embeddingServiceMock = new Mock<IEmbeddingService>();
            _loraServiceMock = new Mock<ILoraService>();
            _httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
        }

        [Fact]
        public void Constructor_WithStableDiffusionServices_CreatesClientSuccessfully()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            // Act
            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Assert
            client.Should().NotBeNull();
            client.TextToImage.Should().BeSameAs(_textToImageMock.Object);
            client.ImageToImage.Should().BeSameAs(_imageToImageMock.Object);
            client.Models.Should().BeSameAs(_modelServiceMock.Object);
            client.Progress.Should().BeSameAs(_progressServiceMock.Object);
            client.Options.Should().BeSameAs(_optionsServiceMock.Object);
            client.Samplers.Should().BeSameAs(_samplerServiceMock.Object);
            client.Schedulers.Should().BeSameAs(_schedulerServiceMock.Object);
            client.Upscalers.Should().BeSameAs(_upscalerServiceMock.Object);
            client.PngInfo.Should().BeSameAs(_pngInfoServiceMock.Object);
            client.Extra.Should().BeSameAs(_extraServiceMock.Object);
            client.Embeddings.Should().BeSameAs(_embeddingServiceMock.Object);
            client.Loras.Should().BeSameAs(_loraServiceMock.Object);
        }

        [Fact]
        public void Constructor_WithNullServices_ThrowsArgumentNullException()
        {
            // Act
            // Создание объекта здесь нужно для проверки исключения в конструкторе
#pragma warning disable IDE0058,CA1806 // Expression value is never used
            Action act = () =>
                new StableDiffusionClient(null!, _httpClientWrapperMock.Object, _loggerMock.Object);
#pragma warning restore IDE0058,CA1806

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void Constructor_WithServicesWithNullTextToImage_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = null!, // Намеренно null
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            // Act
            // Создание объекта здесь нужно для проверки исключения в конструкторе
#pragma warning disable IDE0058,CA1806 // Expression value is never used
            Action act = () =>
                new StableDiffusionClient(
                    services,
                    _httpClientWrapperMock.Object,
                    _loggerMock.Object
                );
#pragma warning restore IDE0058,CA1806

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("TextToImage");
        }

        [Fact]
        public void StableDiffusionServices_Validate_WithAllServicesSet_DoesNotThrow()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            // Act
            Action act = () => services.Validate();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void StableDiffusionServices_Validate_WithNullService_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = null!, // Намеренно null для теста
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            // Act
            Action act = () => services.Validate();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("TextToImage");
        }

        #region HealthCheck Tests

        [Fact]
        public async Task HealthCheckAsync_WithSuccessfulResponse_ReturnsHealthyResult()
        {
            // Arrange
            _httpClientWrapperMock
                .Setup(x => x.GetAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new object());

            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await client.HealthCheckAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsHealthy.Should().BeTrue();
            result.ResponseTime.Should().NotBeNull();
            result.ResponseTime.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
            result.Error.Should().BeNull();
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.Endpoint.Should().Be("/internal/ping");
        }

        [Fact]
        public async Task HealthCheckAsync_WithException_ReturnsUnhealthyResult()
        {
            // Arrange
            var expectedException = new InvalidOperationException("API недоступно");
            _httpClientWrapperMock
                .Setup(x => x.GetAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await client.HealthCheckAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsHealthy.Should().BeFalse();
            result.ResponseTime.Should().BeNull();
            result.Error.Should().Be("API недоступно");
            result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.Endpoint.Should().Be("/internal/ping");

            // Проверяем, что была попытка логирования ошибки (используем базовый метод Log)
            _loggerMock.Verify(
                x => x.Log(LogLevel.Error, expectedException, "API health check failed"),
                Times.Once
            );
        }

        [Fact]
        public async Task HealthCheckAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
            await cts.CancelAsync();
#else
            cts.Cancel();
#endif

            _httpClientWrapperMock
                .Setup(x => x.GetAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Act
            var result = await client.HealthCheckAsync(cts.Token);

            // Assert
            result.Should().NotBeNull();
            result.IsHealthy.Should().BeFalse();
            result.Error.Should().Contain("canceled");
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_ReleasesResources()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Act
            client.Dispose();

            // Assert
            _httpClientWrapperMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_CalledMultipleTimes_DisposesOnlyOnce()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Act - намеренно вызываем Dispose несколько раз для проверки защиты от повторного освобождения
#pragma warning disable S3966 // Objects should not be disposed more than once
            client.Dispose();
            client.Dispose();
            client.Dispose();
#pragma warning restore S3966

            // Assert
            _httpClientWrapperMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_WithAdditionalDisposable_DisposesAllResources()
        {
            // Arrange
            var additionalDisposableMock = new Mock<IDisposable>();
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object,
                additionalDisposableMock.Object
            );

            // Act
            client.Dispose();

            // Assert
            _httpClientWrapperMock.Verify(x => x.Dispose(), Times.Once);
            additionalDisposableMock.Verify(x => x.Dispose(), Times.Once);
        }

#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
        [Fact]
        public async Task DisposeAsync_ReleasesResources()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Act
            await client.DisposeAsync();

            // Assert
            _httpClientWrapperMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_CalledMultipleTimes_DisposesOnlyOnce()
        {
            // Arrange
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object
            );

            // Act - намеренно вызываем DisposeAsync несколько раз для проверки защиты от повторного освобождения
#pragma warning disable S3966 // Objects should not be disposed more than once
            await client.DisposeAsync();
            await client.DisposeAsync();
            await client.DisposeAsync();
#pragma warning restore S3966

            // Assert
            _httpClientWrapperMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_WithAdditionalDisposable_DisposesAllResources()
        {
            // Arrange
            var additionalDisposableMock = new Mock<IDisposable>();
            var services = new StableDiffusionServices
            {
                TextToImage = _textToImageMock.Object,
                ImageToImage = _imageToImageMock.Object,
                Models = _modelServiceMock.Object,
                Progress = _progressServiceMock.Object,
                Options = _optionsServiceMock.Object,
                Samplers = _samplerServiceMock.Object,
                Schedulers = _schedulerServiceMock.Object,
                Upscalers = _upscalerServiceMock.Object,
                PngInfo = _pngInfoServiceMock.Object,
                Extra = _extraServiceMock.Object,
                Embeddings = _embeddingServiceMock.Object,
                Loras = _loraServiceMock.Object,
            };

            var client = new StableDiffusionClient(
                services,
                _httpClientWrapperMock.Object,
                _loggerMock.Object,
                additionalDisposableMock.Object
            );

            // Act
            await client.DisposeAsync();

            // Assert
            _httpClientWrapperMock.Verify(x => x.Dispose(), Times.Once);
            additionalDisposableMock.Verify(x => x.Dispose(), Times.Once);
        }
#endif

        #endregion
    }
}
