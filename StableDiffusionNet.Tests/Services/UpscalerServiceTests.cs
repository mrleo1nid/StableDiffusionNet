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
            var modes = new List<LatentUpscaleMode>
            {
                new LatentUpscaleMode { Name = "Latent" },
                new LatentUpscaleMode { Name = "Latent (antialiased)" },
                new LatentUpscaleMode { Name = "Latent (bicubic)" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        "/sdapi/v1/latent-upscale-modes",
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modes);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("Latent");
            result[1].Name.Should().Be("Latent (antialiased)");
            result[2].Name.Should().Be("Latent (bicubic)");
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_WithDifferentModes_ReturnsAll()
        {
            // Arrange
            var modes = new List<LatentUpscaleMode>
            {
                new LatentUpscaleMode { Name = "Latent" },
                new LatentUpscaleMode { Name = "Latent (nearest)" },
                new LatentUpscaleMode { Name = "Latent (bicubic antialiased)" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modes);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("Latent");
            result[1].Name.Should().Be("Latent (nearest)");
            result[2].Name.Should().Be("Latent (bicubic antialiased)");
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var modes = new List<LatentUpscaleMode>();

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modes);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var modes = new List<LatentUpscaleMode> { new LatentUpscaleMode { Name = "test" } };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modes);

            // Act
            var result = await _service.GetLatentUpscaleModesAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<LatentUpscaleMode>>();
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var modes = new List<LatentUpscaleMode> { new LatentUpscaleMode { Name = "test" } };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modes);

            // Act
            await _service.GetLatentUpscaleModesAsync();

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
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
            var modes = new List<LatentUpscaleMode> { new LatentUpscaleMode { Name = "test" } };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modes);

            // Act
            await _service.GetLatentUpscaleModesAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        "/sdapi/v1/latent-upscale-modes",
                        cts.Token
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLatentUpscaleModesAsync_LogsInformation()
        {
            // Arrange
            var modes = new List<LatentUpscaleMode>
            {
                new LatentUpscaleMode { Name = "mode1" },
                new LatentUpscaleMode { Name = "mode2" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<LatentUpscaleMode>>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(modes);

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

        [Fact]
        public async Task GetUpscalersAsync_WithNullFieldsInResponse_HandlesGracefully()
        {
            // Arrange
            var upscalersJson = new JArray
            {
                new JObject
                {
                    ["name"] = null, // null name - будет отфильтрован
                    ["model_name"] = "model1",
                },
                new JObject
                {
                    ["name"] = "valid_upscaler",
                    ["model_name"] = null, // null model_name - допустимо
                    ["model_path"] = null, // null model_path - допустимо
                    ["model_url"] = null, // null model_url - допустимо
                    ["scale"] = null, // null scale - допустимо
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
            result.Should().HaveCount(1); // Только valid_upscaler, первый отфильтрован
            result[0].Name.Should().Be("valid_upscaler");
            result[0].ModelName.Should().BeEmpty(); // ToString() на null JToken возвращает пустую строку
            result[0].ModelPath.Should().BeEmpty();
            result[0].ModelUrl.Should().BeEmpty();
            result[0].Scale.Should().BeNull(); // ToObject<double?>() на null возвращает null
        }
    }
}
