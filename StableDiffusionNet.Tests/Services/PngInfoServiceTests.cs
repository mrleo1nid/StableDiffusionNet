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
    /// Тесты для PngInfoService
    /// </summary>
    public class PngInfoServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly PngInfoService _service;

        public PngInfoServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new PngInfoService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new PngInfoService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new PngInfoService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetPngInfoAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = async () => await _service.GetPngInfoAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
        }

        [Fact]
        public async Task GetPngInfoAsync_WithValidRequest_ReturnsResponse()
        {
            // Arrange
            var request = new PngInfoRequest { Image = "base64_image_data" };
            var expectedResponse = new PngInfoResponse
            {
                Info = "test image\nSteps: 20, Sampler: Euler a",
                Items = new { prompt = "test" },
            };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<PngInfoRequest, PngInfoResponse>(
                        "/sdapi/v1/png-info",
                        request,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.GetPngInfoAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Info.Should().Be("test image\nSteps: 20, Sampler: Euler a");
            result.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPngInfoAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var request = new PngInfoRequest { Image = "test_image" };
            var response = new PngInfoResponse { Info = "test" };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<PngInfoRequest, PngInfoResponse>(
                        It.IsAny<string>(),
                        It.IsAny<PngInfoRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GetPngInfoAsync(request);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<PngInfoRequest, PngInfoResponse>(
                        "/sdapi/v1/png-info",
                        request,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetPngInfoAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var request = new PngInfoRequest { Image = "test" };
            var response = new PngInfoResponse { Info = "test" };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<PngInfoRequest, PngInfoResponse>(
                        It.IsAny<string>(),
                        It.IsAny<PngInfoRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GetPngInfoAsync(request, cts.Token);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync<PngInfoRequest, PngInfoResponse>(
                        "/sdapi/v1/png-info",
                        request,
                        cts.Token
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetPngInfoAsync_LogsInformation()
        {
            // Arrange
            var request = new PngInfoRequest { Image = "test" };
            var response = new PngInfoResponse { Info = "test" };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync<PngInfoRequest, PngInfoResponse>(
                        It.IsAny<string>(),
                        It.IsAny<PngInfoRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            // Act
            await _service.GetPngInfoAsync(request);

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.Is<string>(s => s.Contains("Extracting PNG"))),
                Times.Once
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.Is<string>(s => s.Contains("extracted successfully"))
                    ),
                Times.Once
            );
        }
    }
}
