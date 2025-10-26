using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;
using StableDiffusionNet.Services;

namespace StableDiffusionNet.Tests.Services
{
    /// <summary>
    /// Тесты для UpscalerService
    /// </summary>
    public class UpscalerServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly UpscalerService _service;

        public UpscalerServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new UpscalerService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new UpscalerService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new UpscalerService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetUpscalersAsync_ReturnsListOfUpscalers()
        {
            // Arrange
            var upscalersJson = new JArray
            {
                new JObject
                {
                    ["name"] = "Lanczos",
                    ["model_path"] = null,
                    ["model_url"] = null,
                    ["scale"] = 4.0,
                },
                new JObject
                {
                    ["name"] = "ESRGAN_4x",
                    ["model_path"] = "/models/ESRGAN/esrgan_4x.pth",
                    ["model_url"] = "https://example.com/esrgan",
                    ["scale"] = 4.0,
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<JArray>("/sdapi/v1/upscalers", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(upscalersJson);

            // Act
            var result = await _service.GetUpscalersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            result[0].Name.Should().Be("Lanczos");
            result[0].ModelPath.Should().BeNullOrEmpty();
            result[0].Scale.Should().Be(4.0);

            result[1].Name.Should().Be("ESRGAN_4x");
            result[1].ModelPath.Should().Be("/models/ESRGAN/esrgan_4x.pth");
            result[1].ModelUrl.Should().Be("https://example.com/esrgan");
        }

        [Fact]
        public async Task GetUpscalersAsync_WithEmptyName_FiltersOut()
        {
            // Arrange
            var upscalersJson = new JArray
            {
                new JObject { ["name"] = "Lanczos" },
                new JObject { ["name"] = "" },
                new JObject { ["name"] = "ESRGAN" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upscalersJson);

            // Act
            var result = await _service.GetUpscalersAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Lanczos");
            result[1].Name.Should().Be("ESRGAN");
        }

        [Fact]
        public async Task GetUpscalersAsync_WithNullName_FiltersOut()
        {
            // Arrange
            var upscalersJson = new JArray
            {
                new JObject { ["name"] = "Lanczos" },
                new JObject { ["name"] = null },
                new JObject { ["name"] = "ESRGAN" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upscalersJson);

            // Act
            var result = await _service.GetUpscalersAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUpscalersAsync_WithEmptyArray_ReturnsEmptyList()
        {
            // Arrange
            var upscalersJson = new JArray();

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upscalersJson);

            // Act
            var result = await _service.GetUpscalersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUpscalersAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var upscalersJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upscalersJson);

            // Act
            var result = await _service.GetUpscalersAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<Upscaler>>();
        }

        [Fact]
        public async Task GetUpscalersAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var upscalersJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upscalersJson);

            // Act
            await _service.GetUpscalersAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/upscalers", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetUpscalersAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var upscalersJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upscalersJson);

            // Act
            await _service.GetUpscalersAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/upscalers", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetUpscalersAsync_LogsInformation()
        {
            // Arrange
            var upscalersJson = new JArray
            {
                new JObject { ["name"] = "upscaler1" },
                new JObject { ["name"] = "upscaler2" },
                new JObject { ["name"] = "upscaler3" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upscalersJson);

            // Act
            await _service.GetUpscalersAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.Is<string>(s => s.Contains("Getting list"))),
                Times.Once
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.Is<string>(s => s.Contains("Upscalers retrieved: 3"))
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_ReturnsListOfModes()
        {
            // Arrange
            var modesJson = new JArray
            {
                new JObject { ["name"] = "Latent" },
                new JObject { ["name"] = "Latent (antialiased)" },
                new JObject { ["name"] = "Latent (bicubic)" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<JArray>(
                        "/sdapi/v1/latent-upscale-modes",
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modesJson);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be("Latent");
            result[1].Should().Be("Latent (antialiased)");
            result[2].Should().Be("Latent (bicubic)");
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_WithEmptyName_FiltersOut()
        {
            // Arrange
            var modesJson = new JArray
            {
                new JObject { ["name"] = "Latent" },
                new JObject { ["name"] = "" },
                new JObject { ["name"] = "Bicubic" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(modesJson);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_WithNullName_FiltersOut()
        {
            // Arrange
            var modesJson = new JArray
            {
                new JObject { ["name"] = "Latent" },
                new JObject { ["name"] = null },
                new JObject { ["name"] = "Bicubic" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(modesJson);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var modesJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(modesJson);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<string>>();
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var modesJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(modesJson);

            // Act
            await _service.GetLatentUpscaleModesAsync();

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.GetAsync<JArray>(
                        "/sdapi/v1/latent-upscale-modes",
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var modesJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(modesJson);

            // Act
            await _service.GetLatentUpscaleModesAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/latent-upscale-modes", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_LogsInformation()
        {
            // Arrange
            var modesJson = new JArray
            {
                new JObject { ["name"] = "mode1" },
                new JObject { ["name"] = "mode2" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(modesJson);

            // Act
            await _service.GetLatentUpscaleModesAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.Is<string>(s => s.Contains("Getting list"))),
                Times.Once
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.Is<string>(s => s.Contains("Latent upscale modes retrieved: 2"))
                    ),
                Times.Once
            );
        }
    }
}
