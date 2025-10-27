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
    /// Тесты для LoraService
    /// </summary>
    public class LoraServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly LoraService _service;

        public LoraServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new LoraService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new LoraService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new LoraService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetLorasAsync_ReturnsListOfLoras()
        {
            // Arrange
            var lorasJson = new JArray
            {
                new JObject
                {
                    ["name"] = "add_detail",
                    ["alias"] = "add-detail",
                    ["path"] = "/models/Lora/add_detail.safetensors",
                    ["metadata"] = new JObject { ["description"] = "Adds details" },
                },
                new JObject
                {
                    ["name"] = "realistic_skin",
                    ["alias"] = "realistic-skin",
                    ["path"] = "/models/Lora/realistic_skin.safetensors",
                },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>("/sdapi/v1/loras", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            var result = await _service.GetLorasAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            result[0].Name.Should().Be("add_detail");
            result[0].Alias.Should().Be("add-detail");
            result[0].Path.Should().Be("/models/Lora/add_detail.safetensors");
            result[0].Metadata.Should().NotBeNull();

            result[1].Name.Should().Be("realistic_skin");
            result[1].Alias.Should().Be("realistic-skin");
        }

        [Fact]
        public async Task GetLorasAsync_WithEmptyName_FiltersOut()
        {
            // Arrange
            var lorasJson = new JArray
            {
                new JObject { ["name"] = "valid_lora" },
                new JObject { ["name"] = "" },
                new JObject { ["name"] = "another_lora" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            var result = await _service.GetLorasAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("valid_lora");
            result[1].Name.Should().Be("another_lora");
        }

        [Fact]
        public async Task GetLorasAsync_WithNullName_FiltersOut()
        {
            // Arrange
            var lorasJson = new JArray
            {
                new JObject { ["name"] = "valid_lora" },
                new JObject { ["name"] = null },
                new JObject { ["name"] = "another_lora" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            var result = await _service.GetLorasAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetLorasAsync_WithEmptyArray_ReturnsEmptyList()
        {
            // Arrange
            var lorasJson = new JArray();

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            var result = await _service.GetLorasAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLorasAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var lorasJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            var result = await _service.GetLorasAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<Lora>>();
        }

        [Fact]
        public async Task GetLorasAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var lorasJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            await _service.GetLorasAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/loras", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLorasAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var lorasJson = new JArray { new JObject { ["name"] = "test" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            await _service.GetLorasAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/loras", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLorasAsync_LogsInformation()
        {
            // Arrange
            var lorasJson = new JArray
            {
                new JObject { ["name"] = "lora1" },
                new JObject { ["name"] = "lora2" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            await _service.GetLorasAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.Is<string>(s => s.Contains("Getting list"))),
                Times.Once
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.Is<string>(s => s.Contains("LoRA models retrieved: 2"))
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshLorasAsync_CallsCorrectEndpoint()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshLorasAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/refresh-loras", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshLorasAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshLorasAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/refresh-loras", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshLorasAsync_LogsInformation()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshLorasAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.Is<string>(s => s.Contains("Refreshing"))),
                Times.Once
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.Is<string>(s => s.Contains("refreshed successfully"))
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLorasAsync_WithNullFieldsInResponse_HandlesGracefully()
        {
            // Arrange
            var lorasJson = new JArray
            {
                new JObject
                {
                    ["name"] = null, // null name - будет отфильтрован
                    ["alias"] = "test-alias",
                },
                new JObject
                {
                    ["name"] = "valid_lora",
                    ["alias"] = null, // null alias - допустимо
                    ["path"] = null, // null path - допустимо
                    ["metadata"] = null, // null metadata - допустимо
                },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>("/sdapi/v1/loras", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lorasJson);

            // Act
            var result = await _service.GetLorasAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1); // Только valid_lora, первый отфильтрован
            result[0].Name.Should().Be("valid_lora");
            result[0].Alias.Should().BeEmpty(); // ToString() на null JToken возвращает пустую строку
            result[0].Path.Should().BeEmpty();
            // metadata это JToken, но в случае null получается пустой JToken
            result[0].Metadata.Should().NotBeNull();
        }
    }
}
