using FluentAssertions;
using Moq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;
using StableDiffusionNet.Services;

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
        public async Task GetSamplersAsync_ReturnsListOfSamplers()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler
                {
                    Name = "Euler a",
                    Aliases = new List<string> { "euler_ancestral" },
                    Options = new Dictionary<string, object>(),
                },
                new Sampler
                {
                    Name = "Euler",
                    Aliases = new List<string> { "euler" },
                    Options = new Dictionary<string, object>(),
                },
                new Sampler
                {
                    Name = "DPM++ 2M Karras",
                    Aliases = new List<string>(),
                    Options = new Dictionary<string, object>(),
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>("/sdapi/v1/samplers", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("Euler a");
            result[0].Aliases.Should().Contain("euler_ancestral");
            result[1].Name.Should().Be("Euler");
            result[1].Aliases.Should().Contain("euler");
            result[2].Name.Should().Be("DPM++ 2M Karras");
        }

        [Fact]
        public async Task GetSamplersAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler { Name = "Euler", Aliases = new List<string>() },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<Sampler>>();
        }

        [Fact]
        public async Task GetSamplersAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler { Name = "Euler", Aliases = new List<string>() },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            await _service.GetSamplersAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<List<Sampler>>("/sdapi/v1/samplers", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSamplersAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var samplers = new List<Sampler>
            {
                new Sampler { Name = "Euler", Aliases = new List<string>() },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            await _service.GetSamplersAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<List<Sampler>>("/sdapi/v1/samplers", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSamplersAsync_WithAliases_ReturnsAliases()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler
                {
                    Name = "Euler a",
                    Aliases = new List<string> { "euler_ancestral", "euler_a" },
                    Options = new Dictionary<string, object>(),
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Euler a");
            result[0].Aliases.Should().HaveCount(2);
            result[0].Aliases.Should().Contain("euler_ancestral");
            result[0].Aliases.Should().Contain("euler_a");
        }

        [Fact]
        public async Task GetSamplersAsync_WithOptions_ReturnsOptions()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler
                {
                    Name = "Euler a",
                    Aliases = new List<string>(),
                    Options = new Dictionary<string, object>
                    {
                        { "discard_next_to_last_sigma", true },
                        { "uses_ensd", true },
                    },
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Options.Should().HaveCount(2);
            result[0].Options.Should().ContainKey("discard_next_to_last_sigma");
            result[0].Options.Should().ContainKey("uses_ensd");
        }

        [Fact]
        public async Task GetSamplersAsync_WithEmptyArray_ReturnsEmptyList()
        {
            // Arrange
            var samplers = new List<Sampler>();

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSamplersAsync_LogsInformation()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler { Name = "Euler", Aliases = new List<string>() },
                new Sampler { Name = "DPM++", Aliases = new List<string>() },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            await _service.GetSamplersAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetSamplersAsync_WithCompleteData_ReturnsAllFields()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler
                {
                    Name = "Euler a",
                    Aliases = new List<string> { "euler_ancestral" },
                    Options = new Dictionary<string, object>
                    {
                        { "discard_next_to_last_sigma", true },
                    },
                },
                new Sampler
                {
                    Name = "DPM++ 2M Karras",
                    Aliases = new List<string>(),
                    Options = new Dictionary<string, object>(),
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(2);

            // First sampler
            result[0].Name.Should().Be("Euler a");
            result[0].Aliases.Should().HaveCount(1);
            result[0].Aliases.Should().Contain("euler_ancestral");
            result[0].Options.Should().HaveCount(1);

            // Second sampler
            result[1].Name.Should().Be("DPM++ 2M Karras");
            result[1].Aliases.Should().BeEmpty();
            result[1].Options.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSamplersAsync_WithEmptyAliasesAndOptions_ReturnsEmptyCollections()
        {
            // Arrange
            var samplers = new List<Sampler>
            {
                new Sampler
                {
                    Name = "Euler",
                    Aliases = new List<string>(),
                    Options = new Dictionary<string, object>(),
                },
                new Sampler
                {
                    Name = "DPM++",
                    Aliases = new List<string>(),
                    Options = new Dictionary<string, object>(),
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Sampler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(samplers);

            // Act
            var result = await _service.GetSamplersAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Euler");
            result[0].Aliases.Should().BeEmpty();
            result[0].Options.Should().BeEmpty();
            result[1].Name.Should().Be("DPM++");
            result[1].Aliases.Should().BeEmpty();
            result[1].Options.Should().BeEmpty();
        }
    }
}
