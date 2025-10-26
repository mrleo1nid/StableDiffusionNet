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
    /// Тесты для ExtraService
    /// </summary>
    public class ExtraServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly ExtraService _service;

        public ExtraServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new ExtraService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ExtraService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ExtraService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task ProcessSingleImageAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = async () => await _service.ProcessSingleImageAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
        }

        [Fact]
        public async Task ProcessSingleImageAsync_WithValidRequest_ReturnsResponse()
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "base64_image_data",
                UpscalingResize = 2,
                Upscaler1 = "Lanczos",
            };

            var expectedResponse = new ExtraSingleImageResponse
            {
                HtmlInfo = "<p>Image processed</p>",
                Image = "processed_base64_image_data",
            };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        "/sdapi/v1/extra-single-image",
                        request,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.ProcessSingleImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Image.Should().Be("processed_base64_image_data");
            result.HtmlInfo.Should().Be("<p>Image processed</p>");
        }

        [Fact]
        public async Task ProcessSingleImageAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var request = new ExtraSingleImageRequest { Image = "test_image", UpscalingResize = 2 };

            var response = new ExtraSingleImageResponse { Image = "processed" };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ExtraSingleImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.ProcessSingleImageAsync(request);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        "/sdapi/v1/extra-single-image",
                        request,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ProcessSingleImageAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var request = new ExtraSingleImageRequest { Image = "test" };
            var response = new ExtraSingleImageResponse { Image = "processed" };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ExtraSingleImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.ProcessSingleImageAsync(request, cts.Token);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        "/sdapi/v1/extra-single-image",
                        request,
                        cts.Token
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ProcessSingleImageAsync_LogsInformation()
        {
            // Arrange
            var request = new ExtraSingleImageRequest { Image = "test" };
            var response = new ExtraSingleImageResponse { Image = "processed" };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ExtraSingleImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.ProcessSingleImageAsync(request);

            // Assert
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Debug,
                        It.Is<string>(s => s.Contains("Processing single image"))
                    ),
                Times.Once
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.Is<string>(s => s.Contains("processed successfully"))
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ProcessSingleImageAsync_WithUpscalerSpecified_UsesUpscaler()
        {
            // Arrange
            var request = new ExtraSingleImageRequest
            {
                Image = "test",
                UpscalingResize = 2,
                Upscaler1 = "ESRGAN_4x",
                Upscaler2 = "Lanczos",
                Upscaler2Visibility = 0.5,
            };

            var response = new ExtraSingleImageResponse { Image = "processed" };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        It.IsAny<string>(),
                        It.IsAny<ExtraSingleImageRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.ProcessSingleImageAsync(request);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<ExtraSingleImageRequest, ExtraSingleImageResponse>(
                        "/sdapi/v1/extra-single-image",
                        It.Is<ExtraSingleImageRequest>(r =>
                            r.Upscaler1 == "ESRGAN_4x" && r.Upscaler2 == "Lanczos"
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
