using FluentAssertions;
using Moq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;
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
        public async Task GetSchedulersAsync_ReturnsListOfSchedulers()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler
                {
                    Name = "automatic",
                    Label = "Automatic",
                    Aliases = null,
                    DefaultRho = -1,
                    NeedInnerModel = false,
                },
                new Scheduler
                {
                    Name = "karras",
                    Label = "Karras",
                    Aliases = null,
                    DefaultRho = 7,
                    NeedInnerModel = false,
                },
                new Scheduler
                {
                    Name = "beta",
                    Label = "Beta",
                    Aliases = null,
                    DefaultRho = -1,
                    NeedInnerModel = true,
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(
                        "/sdapi/v1/schedulers",
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(schedulers);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("automatic");
            result[0].Label.Should().Be("Automatic");
            result[0].DefaultRho.Should().Be(-1);
            result[0].NeedInnerModel.Should().BeFalse();
            result[1].Name.Should().Be("karras");
            result[1].DefaultRho.Should().Be(7);
            result[2].Name.Should().Be("beta");
            result[2].NeedInnerModel.Should().BeTrue();
        }

        [Fact]
        public async Task GetSchedulersAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler { Name = "automatic", Label = "Automatic" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<Scheduler>>();
        }

        [Fact]
        public async Task GetSchedulersAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler { Name = "automatic", Label = "Automatic" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            await _service.GetSchedulersAsync();

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.GetAsync<List<Scheduler>>(
                        "/sdapi/v1/schedulers",
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSchedulersAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var schedulers = new List<Scheduler>
            {
                new Scheduler { Name = "automatic", Label = "Automatic" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            await _service.GetSchedulersAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<List<Scheduler>>("/sdapi/v1/schedulers", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSchedulersAsync_WithAliases_ReturnsAliases()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler
                {
                    Name = "sgm_uniform",
                    Label = "SGM Uniform",
                    Aliases = new List<string> { "SGMUniform" },
                    DefaultRho = -1,
                    NeedInnerModel = true,
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("sgm_uniform");
            result[0].Aliases.Should().NotBeNull();
            result[0].Aliases.Should().Contain("SGMUniform");
        }

        [Fact]
        public async Task GetSchedulersAsync_WithDifferentRhoValues_ReturnsCorrectRho()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler
                {
                    Name = "automatic",
                    Label = "Automatic",
                    DefaultRho = -1,
                },
                new Scheduler
                {
                    Name = "polyexponential",
                    Label = "Polyexponential",
                    DefaultRho = 1,
                },
                new Scheduler
                {
                    Name = "karras",
                    Label = "Karras",
                    DefaultRho = 7,
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(3);
            result[0].DefaultRho.Should().Be(-1);
            result[1].DefaultRho.Should().Be(1);
            result[2].DefaultRho.Should().Be(7);
        }

        [Fact]
        public async Task GetSchedulersAsync_WithEmptyArray_ReturnsEmptyList()
        {
            // Arrange
            var schedulers = new List<Scheduler>();

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSchedulersAsync_LogsInformation()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler { Name = "automatic", Label = "Automatic" },
                new Scheduler { Name = "karras", Label = "Karras" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            await _service.GetSchedulersAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetSchedulersAsync_WithNeedInnerModel_ReturnsCorrectFlags()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler
                {
                    Name = "karras",
                    Label = "Karras",
                    NeedInnerModel = false,
                },
                new Scheduler
                {
                    Name = "normal",
                    Label = "Normal",
                    NeedInnerModel = true,
                },
                new Scheduler
                {
                    Name = "simple",
                    Label = "Simple",
                    NeedInnerModel = true,
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(3);
            result[0].NeedInnerModel.Should().BeFalse();
            result[1].NeedInnerModel.Should().BeTrue();
            result[2].NeedInnerModel.Should().BeTrue();
        }

        [Fact]
        public async Task GetSchedulersAsync_WithCompleteData_ReturnsAllFields()
        {
            // Arrange
            var schedulers = new List<Scheduler>
            {
                new Scheduler
                {
                    Name = "sgm_uniform",
                    Label = "SGM Uniform",
                    Aliases = new List<string> { "SGMUniform" },
                    DefaultRho = -1,
                    NeedInnerModel = true,
                },
                new Scheduler
                {
                    Name = "karras",
                    Label = "Karras",
                    Aliases = null,
                    DefaultRho = 7,
                    NeedInnerModel = false,
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<Scheduler>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(schedulers);

            // Act
            var result = await _service.GetSchedulersAsync();

            // Assert
            result.Should().HaveCount(2);

            // First scheduler
            result[0].Name.Should().Be("sgm_uniform");
            result[0].Label.Should().Be("SGM Uniform");
            result[0].Aliases.Should().NotBeNull();
            result[0].Aliases.Should().HaveCount(1);
            result[0].DefaultRho.Should().Be(-1);
            result[0].NeedInnerModel.Should().BeTrue();

            // Second scheduler
            result[1].Name.Should().Be("karras");
            result[1].Label.Should().Be("Karras");
            result[1].Aliases.Should().BeNull();
            result[1].DefaultRho.Should().Be(7);
            result[1].NeedInnerModel.Should().BeFalse();
        }
    }
}
