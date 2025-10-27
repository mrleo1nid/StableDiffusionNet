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
    /// Тесты для ImageToImageService
    /// </summary>
    public class ImageToImageServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly ImageToImageService _service;

        public ImageToImageServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new ImageToImageService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ImageToImageService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ImageToImageService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GenerateAsync_WithValidRequest_ReturnsResponse()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "base64image" },
                Prompt = "a beautiful landscape",
                DenoisingStrength = 0.75,
            };

            var expectedResponse = new ImageToImageResponse
            {
                Images = new List<string> { "base64result1", "base64result2" },
                Info = "Generation info",
            };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        "/sdapi/v1/img2img",
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
            result.Images![0].Should().Be("base64result1");
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
        public async Task GenerateAsync_WithNullInitImages_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest { InitImages = null!, Prompt = "test" };

            // Act & Assert
            var act = async () => await _service.GenerateAsync(request);
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*At least one initial image must be provided*")
                .WithParameterName("request");
        }

        [Fact]
        public async Task GenerateAsync_WithEmptyInitImages_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string>(),
                Prompt = "test",
            };

            // Act & Assert
            var act = async () => await _service.GenerateAsync(request);
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*At least one initial image must be provided*")
                .WithParameterName("request");
        }

        [Fact]
        public async Task GenerateAsync_WithEmptyPrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = string.Empty,
            };

            // Act & Assert
            var act = async () => await _service.GenerateAsync(request);
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*Prompt cannot be empty*")
                .WithParameterName("request");
        }

        [Fact]
        public async Task GenerateAsync_WithWhitespacePrompt_ThrowsArgumentException()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "   ",
            };

            // Act & Assert
            var act = async () => await _service.GenerateAsync(request);
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("request");
        }

        [Fact]
        public async Task GenerateAsync_CallsHttpClientWithCorrectEndpoint()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test prompt",
            };
            var response = new ImageToImageResponse { Images = new List<string> { "result" } };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ImageToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GenerateAsync(request);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        "/sdapi/v1/img2img",
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
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
            };
            var response = new ImageToImageResponse { Images = new List<string> { "result" } };
            using var cts = new CancellationTokenSource();

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ImageToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GenerateAsync(request, cts.Token);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ImageToImageRequest>(),
                        cts.Token
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateAsync_LogsInformation()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test prompt",
                DenoisingStrength = 0.75,
            };
            var response = new ImageToImageResponse { Images = new List<string> { "result" } };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ImageToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GenerateAsync(request);

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<string>()),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task GenerateAsync_WithMultipleInitImages_AcceptsRequest()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image1", "image2", "image3" },
                Prompt = "test prompt",
            };
            var response = new ImageToImageResponse { Images = new List<string> { "result" } };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ImageToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GenerateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().HaveCount(1);
        }

        [Fact]
        public async Task GenerateAsync_WithMask_PreservesProperty()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "image" },
                Prompt = "test",
                Mask = "mask_base64",
                MaskBlur = 10,
            };

            ImageToImageRequest? capturedRequest = null;
            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ImageToImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Callback<string, ImageToImageRequest, CancellationToken>(
                    (_, req, _) => capturedRequest = req
                )
                .ReturnsAsync(new ImageToImageResponse { Images = new List<string> { "result" } });

            // Act
            await _service.GenerateAsync(request);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Mask.Should().Be("mask_base64");
            capturedRequest.MaskBlur.Should().Be(10);
        }

        [Fact]
        public async Task GenerateAsync_WithNullImagesInResponse_LogsZeroImages()
        {
            // Arrange
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { "base64image" },
                Prompt = "test prompt",
            };

            var responseWithNullImages = new ImageToImageResponse
            {
                Images = null, // Тестируем случай когда Images == null
                Info = "Generation info",
            };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ImageToImageRequest, ImageToImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ImageToImageRequest>(),
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
