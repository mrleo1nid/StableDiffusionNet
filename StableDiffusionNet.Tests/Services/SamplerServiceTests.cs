using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Services;
using Xunit;

namespace StableDiffusionNet.Tests.Services
{
    /// <summary>
    /// Тесты для SamplerService
    /// </summary>
    public class SamplerServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly SamplerService _service;

        public SamplerServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new SamplerService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new SamplerService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new SamplerService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetSamplersAsync_ReturnsListOfSamplerNames()
        {
            // Arrange
            var samplersJson = new JArray
            {
                new JObject
                {
                    ["name"] = "Euler a",
                    ["aliases"] = new JArray { "euler_ancestral" },
                },
                new JObject
                {
                    ["name"] = "Euler",
                    ["aliases"] = new JArray { "euler" },
                },
                new JObject { ["name"] = "DPM++ 2M Karras" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>("/sdapi/v1/samplers", It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be("Euler a");
            result[1].Should().Be("Euler");
            result[2].Should().Be("DPM++ 2M Karras");
        }

        [Fact]
        public async Task GetSamplersAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var samplersJson = new JArray { new JObject { ["name"] = "Euler" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<string>>();
        }

        [Fact]
        public async Task GetSamplersAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var samplersJson = new JArray { new JObject { ["name"] = "Euler" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            await _service.GetSamplersAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/samplers", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSamplersAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var samplersJson = new JArray { new JObject { ["name"] = "Euler" } };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            await _service.GetSamplersAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JArray>("/sdapi/v1/samplers", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSamplersAsync_WithNullName_FiltersOut()
        {
            // Arrange
            var samplersJson = new JArray
            {
                new JObject { ["name"] = "Euler" },
                new JObject { ["name"] = null },
                new JObject { ["name"] = "DPM++" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("Euler");
            result.Should().Contain("DPM++");
        }

        [Fact]
        public async Task GetSamplersAsync_WithEmptyName_FiltersOut()
        {
            // Arrange
            var samplersJson = new JArray
            {
                new JObject { ["name"] = "Euler" },
                new JObject { ["name"] = "" },
                new JObject { ["name"] = "DPM++" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain("");
        }

        [Fact]
        public async Task GetSamplersAsync_WithEmptyArray_ReturnsEmptyList()
        {
            // Arrange
            var samplersJson = new JArray();

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSamplersAsync_LogsInformation()
        {
            // Arrange
            var samplersJson = new JArray
            {
                new JObject { ["name"] = "Euler" },
                new JObject { ["name"] = "DPM++" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            await _service.GetSamplersAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetSamplersAsync_WithComplexSamplerData_ExtractsNamesCorrectly()
        {
            // Arrange
            var samplersJson = new JArray
            {
                new JObject
                {
                    ["name"] = "Euler a",
                    ["aliases"] = new JArray { "euler_ancestral" },
                    ["options"] = new JObject { ["discard_next_to_last_sigma"] = true },
                },
                new JObject
                {
                    ["name"] = "DPM++ 2M Karras",
                    ["aliases"] = new JArray(),
                    ["options"] = new JObject(),
                },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().Be("Euler a");
            result[1].Should().Be("DPM++ 2M Karras");
        }

        [Fact]
        public async Task GetSamplersAsync_WithObjectMissingNameProperty_HandlesGracefully()
        {
            // Arrange
            var samplersJson = new JArray
            {
                new JObject { ["name"] = "Euler" },
                new JObject { ["otherProperty"] = "value" },
                new JObject { ["name"] = "DPM++" },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JArray>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(samplersJson);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("Euler");
            result.Should().Contain("DPM++");
        }
    }
}
