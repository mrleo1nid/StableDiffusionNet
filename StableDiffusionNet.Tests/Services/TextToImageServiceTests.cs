using FluentAssertions;
using Moq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;
using StableDiffusionNet.Services;

namespace StableDiffusionNet.Tests.Services
{
    /// <summary>
    /// Тесты для TextToImageService
    /// </summary>
    public class TextToImageServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly TextToImageService _service;

        public TextToImageServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new TextToImageService(
                _httpClientMock.Object,
                _loggerMock.Object,
                new StableDiffusionNet.Configuration.ValidationOptions()
            );
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new TextToImageService(
                    null!,
                    _loggerMock.Object,
                    new StableDiffusionNet.Configuration.ValidationOptions()
                );
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () =>
                new TextToImageService(
                    _httpClientMock.Object,
                    null!,
                    new StableDiffusionNet.Configuration.ValidationOptions()
                );
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GenerateAsync_WithValidRequest_ReturnsResponse()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "a beautiful landscape",
                Width = 512,
                Height = 512,
            };

            var expectedResponse = new TextToImageResponse
            {
                Images = new List<string> { "base64image1", "base64image2" },
                Info = "Generation info",
            };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        "/sdapi/v1/txt2img",
                        request,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.GenerateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeNull();
            result.Images.Should().HaveCount(2);
            result.Images![0].Should().Be("base64image1");
            result.Info.Should().Be("Generation info");
        }

        [Fact]
        public async Task GenerateAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = async () => await _service.GenerateAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
        }

        [Fact]
        public async Task GenerateAsync_WithEmptyPrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = string.Empty };

            // Act & Assert
            var act = async () => await _service.GenerateAsync(request);
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null, empty, or whitespace*");
        }

        [Fact]
        public async Task GenerateAsync_WithWhitespacePrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "   " };

            // Act & Assert
            var act = async () => await _service.GenerateAsync(request);
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("request");
        }

        [Fact]
        public async Task GenerateAsync_WithNullPrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = null! };

            // Act & Assert
            var act = async () => await _service.GenerateAsync(request);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GenerateAsync_CallsHttpClientWithCorrectEndpoint()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test prompt" };
            var response = new TextToImageResponse { Images = new List<string> { "image" } };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<TextToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GenerateAsync(request);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        "/sdapi/v1/txt2img",
                        request,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateAsync_WithCancellationToken_PassesTokenToHttpClient()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test prompt" };
            var response = new TextToImageResponse { Images = new List<string> { "image" } };
            using var cts = new CancellationTokenSource();

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<TextToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GenerateAsync(request, cts.Token);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<TextToImageRequest>(),
                        cts.Token
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateAsync_LogsInformation()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "test prompt",
                Width = 512,
                Height = 512,
            };
            var response = new TextToImageResponse { Images = new List<string> { "image" } };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<TextToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GenerateAsync(request);

            // Assert
            _loggerMock.Verify(
                l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log information during generation"
            );
        }

        [Fact]
        public async Task GenerateAsync_WithComplexRequest_PreservesAllProperties()
        {
            // Arrange
            var request = new TextToImageRequest
            {
                Prompt = "complex prompt",
                NegativePrompt = "bad quality",
                Steps = 30,
                SamplerName = "Euler a",
                Width = 768,
                Height = 768,
                CfgScale = 7.5,
                Seed = 12345,
                BatchSize = 2,
                RestoreFaces = true,
                EnableHr = true,
            };

            TextToImageRequest? capturedRequest = null;
            _httpClientMock
                .Setup(x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<TextToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Callback<string, TextToImageRequest, CancellationToken>(
                    (_, req, _) => capturedRequest = req
                )
                .ReturnsAsync(new TextToImageResponse { Images = new List<string> { "image" } });

            // Act
            await _service.GenerateAsync(request);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Prompt.Should().Be("complex prompt");
            capturedRequest.NegativePrompt.Should().Be("bad quality");
            capturedRequest.Steps.Should().Be(30);
            capturedRequest.Width.Should().Be(768);
        }

        [Fact]
        public async Task GenerateAsync_WithNullImagesInResponse_LogsZeroImages()
        {
            // Arrange
            var request = new TextToImageRequest { Prompt = "test prompt" };

            var responseWithNullImages = new TextToImageResponse
            {
                Images = null, // Тестируем случай когда Images == null
                Info = "Generation info",
            };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<TextToImageRequest, TextToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<TextToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(responseWithNullImages);

            // Act
            var result = await _service.GenerateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().BeNull();
            _loggerMock.Verify(
                x =>
                    x.Log(
                        It.IsAny<LogLevel>(),
                        It.Is<string>(s => s.Contains("Images generated: 0"))
                    ),
                Times.AtLeastOnce
            );
        }
    }
}
