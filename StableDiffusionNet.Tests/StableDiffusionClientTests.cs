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
            _loggerMock = new Mock<IStableDiffusionLogger>();
        }

        private StableDiffusionClient CreateClient()
        {
            return new StableDiffusionClient(
                _textToImageMock.Object,
                _imageToImageMock.Object,
                _modelServiceMock.Object,
                _progressServiceMock.Object,
                _optionsServiceMock.Object,
                _samplerServiceMock.Object,
                _schedulerServiceMock.Object,
                _upscalerServiceMock.Object,
                _pngInfoServiceMock.Object,
                _extraServiceMock.Object,
                _embeddingServiceMock.Object,
                _loraServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesClient()
        {
            // Act
            var client = CreateClient();

            // Assert
            client.Should().NotBeNull();
            client.TextToImage.Should().NotBeNull();
            client.ImageToImage.Should().NotBeNull();
            client.Models.Should().NotBeNull();
            client.Progress.Should().NotBeNull();
            client.Options.Should().NotBeNull();
            client.Samplers.Should().NotBeNull();
            client.Schedulers.Should().NotBeNull();
            client.Upscalers.Should().NotBeNull();
            client.PngInfo.Should().NotBeNull();
            client.Extra.Should().NotBeNull();
            client.Embeddings.Should().NotBeNull();
            client.Loras.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullTextToImageService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    null!,
                    _imageToImageMock.Object,
                    _modelServiceMock.Object,
                    _progressServiceMock.Object,
                    _optionsServiceMock.Object,
                    _samplerServiceMock.Object,
                    _schedulerServiceMock.Object,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    _loggerMock.Object
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("textToImageService");
        }

        [Fact]
        public void Constructor_WithNullImageToImageService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    _textToImageMock.Object,
                    null!,
                    _modelServiceMock.Object,
                    _progressServiceMock.Object,
                    _optionsServiceMock.Object,
                    _samplerServiceMock.Object,
                    _schedulerServiceMock.Object,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    _loggerMock.Object
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("imageToImageService");
        }

        [Fact]
        public void Constructor_WithNullModelService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    _textToImageMock.Object,
                    _imageToImageMock.Object,
                    null!,
                    _progressServiceMock.Object,
                    _optionsServiceMock.Object,
                    _samplerServiceMock.Object,
                    _schedulerServiceMock.Object,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    _loggerMock.Object
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("modelService");
        }

        [Fact]
        public void Constructor_WithNullProgressService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    _textToImageMock.Object,
                    _imageToImageMock.Object,
                    _modelServiceMock.Object,
                    null!,
                    _optionsServiceMock.Object,
                    _samplerServiceMock.Object,
                    _schedulerServiceMock.Object,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    _loggerMock.Object
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("progressService");
        }

        [Fact]
        public void Constructor_WithNullOptionsService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    _textToImageMock.Object,
                    _imageToImageMock.Object,
                    _modelServiceMock.Object,
                    _progressServiceMock.Object,
                    null!,
                    _samplerServiceMock.Object,
                    _schedulerServiceMock.Object,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    _loggerMock.Object
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("optionsService");
        }

        [Fact]
        public void Constructor_WithNullSamplerService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    _textToImageMock.Object,
                    _imageToImageMock.Object,
                    _modelServiceMock.Object,
                    _progressServiceMock.Object,
                    _optionsServiceMock.Object,
                    null!,
                    _schedulerServiceMock.Object,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    _loggerMock.Object
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("samplerService");
        }

        [Fact]
        public void Constructor_WithNullSchedulerService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    _textToImageMock.Object,
                    _imageToImageMock.Object,
                    _modelServiceMock.Object,
                    _progressServiceMock.Object,
                    _optionsServiceMock.Object,
                    _samplerServiceMock.Object,
                    null!,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    _loggerMock.Object
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("schedulerService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new StableDiffusionClient(
                    _textToImageMock.Object,
                    _imageToImageMock.Object,
                    _modelServiceMock.Object,
                    _progressServiceMock.Object,
                    _optionsServiceMock.Object,
                    _samplerServiceMock.Object,
                    _schedulerServiceMock.Object,
                    _upscalerServiceMock.Object,
                    _pngInfoServiceMock.Object,
                    _extraServiceMock.Object,
                    _embeddingServiceMock.Object,
                    _loraServiceMock.Object,
                    null!
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void TextToImage_Property_ReturnsInjectedService()
        {
            // Arrange
            var client = CreateClient();

            // Act & Assert
            client.TextToImage.Should().BeSameAs(_textToImageMock.Object);
        }

        [Fact]
        public void ImageToImage_Property_ReturnsInjectedService()
        {
            // Arrange
            var client = CreateClient();

            // Act & Assert
            client.ImageToImage.Should().BeSameAs(_imageToImageMock.Object);
        }

        [Fact]
        public void Models_Property_ReturnsInjectedService()
        {
            // Arrange
            var client = CreateClient();

            // Act & Assert
            client.Models.Should().BeSameAs(_modelServiceMock.Object);
        }

        [Fact]
        public void Progress_Property_ReturnsInjectedService()
        {
            // Arrange
            var client = CreateClient();

            // Act & Assert
            client.Progress.Should().BeSameAs(_progressServiceMock.Object);
        }

        [Fact]
        public void Options_Property_ReturnsInjectedService()
        {
            // Arrange
            var client = CreateClient();

            // Act & Assert
            client.Options.Should().BeSameAs(_optionsServiceMock.Object);
        }

        [Fact]
        public void Samplers_Property_ReturnsInjectedService()
        {
            // Arrange
            var client = CreateClient();

            // Act & Assert
            client.Samplers.Should().BeSameAs(_samplerServiceMock.Object);
        }

        [Fact]
        public void Schedulers_Property_ReturnsInjectedService()
        {
            // Arrange
            var client = CreateClient();

            // Act & Assert
            client.Schedulers.Should().BeSameAs(_schedulerServiceMock.Object);
        }

        [Fact]
        public async Task PingAsync_WhenApiAvailable_ReturnsTrue()
        {
            // Arrange
            _samplerServiceMock
                .Setup(x => x.GetSamplersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sampler> { new Sampler { Name = "Euler" } }.AsReadOnly());

            var client = CreateClient();

            // Act
            var result = await client.PingAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task PingAsync_WhenApiUnavailable_ReturnsFalse()
        {
            // Arrange
            _samplerServiceMock
                .Setup(x => x.GetSamplersAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("API unavailable"));

            var client = CreateClient();

            // Act
            var result = await client.PingAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PingAsync_CallsSamplersService()
        {
            // Arrange
            _samplerServiceMock
                .Setup(x => x.GetSamplersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sampler> { new Sampler { Name = "Euler" } }.AsReadOnly());

            var client = CreateClient();

            // Act
            await client.PingAsync();

            // Assert
            _samplerServiceMock.Verify(
                x => x.GetSamplersAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task PingAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _samplerServiceMock
                .Setup(x => x.GetSamplersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sampler> { new Sampler { Name = "Euler" } }.AsReadOnly());

            var client = CreateClient();

            // Act
            await client.PingAsync(cts.Token);

            // Assert
            _samplerServiceMock.Verify(x => x.GetSamplersAsync(cts.Token), Times.Once);
        }

        [Fact]
        public async Task PingAsync_WhenSuccessful_LogsInformation()
        {
            // Arrange
            _samplerServiceMock
                .Setup(x => x.GetSamplersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sampler> { new Sampler { Name = "Euler" } }.AsReadOnly());

            var client = CreateClient();

            // Act
            var result = await client.PingAsync();

            // Assert
            result.Should().BeTrue();
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task PingAsync_WhenFails_LogsError()
        {
            // Arrange
            var exception = new Exception("Connection failed");
            _samplerServiceMock
                .Setup(x => x.GetSamplersAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var client = CreateClient();

            // Act
            var result = await client.PingAsync();

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<Exception>(), It.IsAny<string>()),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task PingAsync_MultipleCalls_WorksCorrectly()
        {
            // Arrange
            _samplerServiceMock
                .Setup(x => x.GetSamplersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sampler> { new Sampler { Name = "Euler" } }.AsReadOnly());

            var client = CreateClient();

            // Act
            var result1 = await client.PingAsync();
            var result2 = await client.PingAsync();
            var result3 = await client.PingAsync();

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            result3.Should().BeTrue();
            _samplerServiceMock.Verify(
                x => x.GetSamplersAsync(It.IsAny<CancellationToken>()),
                Times.Exactly(3)
            );
        }
    }
}
