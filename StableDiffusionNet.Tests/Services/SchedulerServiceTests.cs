using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Services;

namespace StableDiffusionNet.Tests.Services
{
    /// <summary>
    /// Тесты для SchedulerService
    /// </summary>
    public class SchedulerServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly SchedulerService _service;

        public SchedulerServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new SchedulerService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new SchedulerService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new SchedulerService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetSchedulersAsync_ReturnsListOfSchedulerNames()
        {
            // Arrange
            var schedulersJson = new JArray
            {
                new JObject { ["name"] = "Automatic", ["label"] = "Automatic" },
                new JObject { ["name"] = "Uniform", ["label"] = "Uniform" },
                new JObject { ["name"] = "Karras" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<JArray>("/sdapi/v1/schedulers", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulersJson);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be("Automatic");
            result[1].Should().Be("Uniform");
            result[2].Should().Be("Karras");
        }

        [Fact]
        public async Task GetSchedulersAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var schedulersJson = new JArray { new JObject { ["name"] = "Automatic" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<string>>();
        }

        [Fact]
        public async Task GetSchedulersAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var schedulersJson = new JArray { new JObject { ["name"] = "Automatic" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            await _service.GetSchedulersAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/schedulers", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSchedulersAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var schedulersJson = new JArray { new JObject { ["name"] = "Automatic" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            await _service.GetSchedulersAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/schedulers", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSchedulersAsync_WithNullName_FiltersOut()
        {
            // Arrange
            var schedulersJson = new JArray
            {
                new JObject { ["name"] = "Automatic" },
                new JObject { ["name"] = null },
                new JObject { ["name"] = "Karras" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("Automatic");
            result.Should().Contain("Karras");
        }

        [Fact]
        public async Task GetSchedulersAsync_WithEmptyName_FiltersOut()
        {
            // Arrange
            var schedulersJson = new JArray
            {
                new JObject { ["name"] = "Automatic" },
                new JObject { ["name"] = "" },
                new JObject { ["name"] = "Karras" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain("");
        }

        [Fact]
        public async Task GetSchedulersAsync_WithEmptyArray_ReturnsEmptyList()
        {
            // Arrange
            var schedulersJson = new JArray();

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSchedulersAsync_LogsInformation()
        {
            // Arrange
            var schedulersJson = new JArray
            {
                new JObject { ["name"] = "Automatic" },
                new JObject { ["name"] = "Karras" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            await _service.GetSchedulersAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetSchedulersAsync_WithComplexSchedulerData_ExtractsNamesCorrectly()
        {
            // Arrange
            var schedulersJson = new JArray
            {
                new JObject
                {
                    ["name"] = "Automatic",
                    ["label"] = "Automatic",
                    ["aliases"] = new JArray(),
                },
                new JObject
                {
                    ["name"] = "Karras",
                    ["label"] = "Karras",
                    ["aliases"] = new JArray { "karras" },
                },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().Be("Automatic");
            result[1].Should().Be("Karras");
        }

        [Fact]
        public async Task GetSchedulersAsync_WithObjectMissingNameProperty_HandlesGracefully()
        {
            // Arrange
            var schedulersJson = new JArray
            {
                new JObject { ["name"] = "Automatic" },
                new JObject { ["otherProperty"] = "value" },
                new JObject { ["name"] = "Karras" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedulersJson);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("Automatic");
            result.Should().Contain("Karras");
        }
    }
}
