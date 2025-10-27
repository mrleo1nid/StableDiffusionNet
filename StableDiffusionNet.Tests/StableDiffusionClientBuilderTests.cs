using FluentAssertions;
using Moq;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests
{
    /// <summary>
    /// Тесты для StableDiffusionClientBuilder
    /// </summary>
    public class StableDiffusionClientBuilderTests
    {
        [Fact]
        public void CreateDefault_CreatesClient_Successfully()
        {
            // Act
            var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");

            // Assert
            client.Should().NotBeNull();
            client.Should().BeAssignableTo<IStableDiffusionClient>();
            client.TextToImage.Should().NotBeNull();
            client.ImageToImage.Should().NotBeNull();
            client.Models.Should().NotBeNull();
            client.Progress.Should().NotBeNull();
            client.Options.Should().NotBeNull();
            client.Samplers.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithCustomOptions_CreatesClient_Successfully()
        {
            // Arrange
            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithTimeout(600)
                .WithRetry(5, 2000)
                .WithDetailedLogging();

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
            client.Should().BeAssignableTo<IStableDiffusionClient>();
        }

        [Fact]
        public void Build_WithLoggerFactory_CreatesClient_Successfully()
        {
            // Arrange
            var logger = Mock.Of<IStableDiffusionLogger>();
            var loggerFactoryMock = new Mock<IStableDiffusionLoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger);
            loggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(logger);

            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithLoggerFactory(loggerFactoryMock.Object);

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithApiKey_CreatesClient_Successfully()
        {
            // Arrange
            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithApiKey("test-api-key");

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithStableDiffusionOptions_CreatesClient_Successfully()
        {
            // Arrange
            var options = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                TimeoutSeconds = 600,
                RetryCount = 5,
            };

            var builder = new StableDiffusionClientBuilder().WithOptions(options);

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
        }

        [Fact]
        public void Build_WithInvalidBaseUrl_ThrowsException()
        {
            // Arrange & Act
            Action act = () => new StableDiffusionClientBuilder().WithBaseUrl("invalid-url");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*BaseUrl must be a valid URL*");
        }

        [Fact]
        public void Build_WithEmptyBaseUrl_ThrowsException()
        {
            // Arrange & Act
            Action act = () => new StableDiffusionClientBuilder().WithBaseUrl("");

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*")
                .WithParameterName("BaseUrl");
        }

        [Fact]
        public void Build_WithNegativeTimeout_ThrowsException()
        {
            // Arrange & Act
            Action act = () =>
                new StableDiffusionClientBuilder()
                    .WithBaseUrl("http://localhost:7860")
                    .WithTimeout(-1);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*TimeoutSeconds must be a positive number*");
        }

        [Fact]
        public void Build_WithNegativeRetryCount_ThrowsException()
        {
            // Arrange & Act
            Action act = () =>
                new StableDiffusionClientBuilder()
                    .WithBaseUrl("http://localhost:7860")
                    .WithRetry(-1);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*RetryCount cannot be negative*");
        }

        [Fact]
        public void WithOptions_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new StableDiffusionClientBuilder();

            // Act
            Action act = () => builder.WithOptions(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }

        [Fact]
        public void Build_WithCustomHttpClient_CreatesClient_Successfully()
        {
            // Arrange
            var customHttpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:7860"),
                Timeout = TimeSpan.FromSeconds(600),
            };

            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithHttpClient(customHttpClient);

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
            client.Should().BeAssignableTo<IStableDiffusionClient>();
        }

        [Fact]
        public void WithHttpClient_SetsHttpClient_ReturnsBuilder()
        {
            // Arrange
            var customHttpClient = new HttpClient();
            var builder = new StableDiffusionClientBuilder().WithBaseUrl("http://localhost:7860");

            // Act
            var result = builder.WithHttpClient(customHttpClient);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(builder); // Fluent API должен возвращать тот же экземпляр
        }

        [Fact]
        public void WithServiceFactory_WithNullFactory_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new StableDiffusionClientBuilder();

            // Act
            Action act = () => builder.WithServiceFactory(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("serviceFactory");
        }

        [Fact]
        public void WithServiceFactory_SetsFactory_ReturnsBuilder()
        {
            // Arrange
            var mockFactory = new Mock<IStableDiffusionServiceFactory>();
            var builder = new StableDiffusionClientBuilder().WithBaseUrl("http://localhost:7860");

            // Act
            var result = builder.WithServiceFactory(mockFactory.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(builder); // Fluent API должен возвращать тот же экземпляр
        }

        [Fact]
        public void Build_WithCustomServiceFactory_UsesCustomFactory()
        {
            // Arrange
            var mockLogger = Mock.Of<IStableDiffusionLogger>();
            var mockLoggerFactory = new Mock<IStableDiffusionLoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger);
            mockLoggerFactory.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(mockLogger);

            var mockTextToImageService = Mock.Of<ITextToImageService>();
            var mockImageToImageService = Mock.Of<IImageToImageService>();
            var mockModelService = Mock.Of<IModelService>();
            var mockProgressService = Mock.Of<IProgressService>();
            var mockOptionsService = Mock.Of<IOptionsService>();
            var mockSamplerService = Mock.Of<ISamplerService>();
            var mockSchedulerService = Mock.Of<ISchedulerService>();
            var mockUpscalerService = Mock.Of<IUpscalerService>();
            var mockPngInfoService = Mock.Of<IPngInfoService>();
            var mockExtraService = Mock.Of<IExtraService>();
            var mockEmbeddingService = Mock.Of<IEmbeddingService>();
            var mockLoraService = Mock.Of<ILoraService>();

            var mockServiceFactory = new Mock<IStableDiffusionServiceFactory>();
            mockServiceFactory
                .Setup(f =>
                    f.CreateTextToImageService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockTextToImageService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateImageToImageService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockImageToImageService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateModelService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockModelService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateProgressService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockProgressService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateOptionsService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockOptionsService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateSamplerService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockSamplerService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateSchedulerService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockSchedulerService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateUpscalerService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockUpscalerService);
            mockServiceFactory
                .Setup(f =>
                    f.CreatePngInfoService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockPngInfoService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateExtraService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockExtraService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateEmbeddingService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockEmbeddingService);
            mockServiceFactory
                .Setup(f =>
                    f.CreateLoraService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    )
                )
                .Returns(mockLoraService);

            var builder = new StableDiffusionClientBuilder()
                .WithBaseUrl("http://localhost:7860")
                .WithLoggerFactory(mockLoggerFactory.Object)
                .WithServiceFactory(mockServiceFactory.Object);

            // Act
            var client = builder.Build();

            // Assert
            client.Should().NotBeNull();
            client.TextToImage.Should().BeSameAs(mockTextToImageService);
            client.ImageToImage.Should().BeSameAs(mockImageToImageService);
            client.Models.Should().BeSameAs(mockModelService);
            client.Progress.Should().BeSameAs(mockProgressService);
            client.Options.Should().BeSameAs(mockOptionsService);
            client.Samplers.Should().BeSameAs(mockSamplerService);
            client.Schedulers.Should().BeSameAs(mockSchedulerService);
            client.Upscalers.Should().BeSameAs(mockUpscalerService);
            client.PngInfo.Should().BeSameAs(mockPngInfoService);
            client.Extra.Should().BeSameAs(mockExtraService);
            client.Embeddings.Should().BeSameAs(mockEmbeddingService);
            client.Loras.Should().BeSameAs(mockLoraService);

            // Проверяем что фабрика была вызвана для каждого сервиса
            mockServiceFactory.Verify(
                f =>
                    f.CreateTextToImageService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    ),
                Times.Once
            );
            mockServiceFactory.Verify(
                f =>
                    f.CreateImageToImageService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    ),
                Times.Once
            );
            mockServiceFactory.Verify(
                f =>
                    f.CreateModelService(
                        It.IsAny<IHttpClientWrapper>(),
                        It.IsAny<IStableDiffusionLogger>(),
                        It.IsAny<StableDiffusionNet.Configuration.ValidationOptions>()
                    ),
                Times.Once
            );
        }
    }
}
