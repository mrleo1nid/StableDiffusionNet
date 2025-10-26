using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;
using StableDiffusionNet.Services;
using Xunit;

namespace StableDiffusionNet.Tests.Services
{
    /// <summary>
    /// Тесты для ProgressService
    /// </summary>
    public class ProgressServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly ProgressService _service;

        public ProgressServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new ProgressService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ProgressService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ProgressService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetProgressAsync_ReturnsProgress()
        {
            // Arrange
            var expectedProgress = new GenerationProgress
            {
                Progress = 0.5,
                EtaRelative = 30.0,
                State = new ProgressState
                {
                    SamplingStep = 10,
                    SamplingSteps = 20,
                    Job = "txt2img",
                    JobCount = 1,
                    JobNo = 0,
                },
                CurrentImage = "base64image",
                TextInfo = "Generating...",
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<GenerationProgress>(
                        "/sdapi/v1/progress",
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(expectedProgress);

            // Act
            var result = await _service.GetProgressAsync();

            // Assert
            result.Should().NotBeNull();
            result.Progress.Should().Be(0.5);
            result.EtaRelative.Should().Be(30.0);
            result.State.Should().NotBeNull();
            result.State!.SamplingStep.Should().Be(10);
            result.State.SamplingSteps.Should().Be(20);
        }

        [Fact]
        public async Task GetProgressAsync_WithNullState_HandlesGracefully()
        {
            // Arrange
            var progress = new GenerationProgress { Progress = 0.0, State = null };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<GenerationProgress>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(progress);

            // Act
            var result = await _service.GetProgressAsync();

            // Assert
            result.Should().NotBeNull();
            result.State.Should().BeNull();
        }

        [Fact]
        public async Task GetProgressAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var progress = new GenerationProgress { Progress = 0.5 };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<GenerationProgress>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(progress);

            // Act
            await _service.GetProgressAsync();

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.GetAsync<GenerationProgress>(
                        "/sdapi/v1/progress",
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProgressAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var progress = new GenerationProgress { Progress = 0.5 };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<GenerationProgress>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(progress);

            // Act
            await _service.GetProgressAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<GenerationProgress>("/sdapi/v1/progress", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProgressAsync_WithState_LogsDebug()
        {
            // Arrange
            var progress = new GenerationProgress
            {
                Progress = 0.5,
                State = new ProgressState { SamplingStep = 10, SamplingSteps = 20 },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<GenerationProgress>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(progress);

            // Act
            await _service.GetProgressAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task InterruptAsync_CallsCorrectEndpoint()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync("/sdapi/v1/interrupt", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.InterruptAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/interrupt", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task InterruptAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();

            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.InterruptAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(x => x.PostAsync("/sdapi/v1/interrupt", cts.Token), Times.Once);
        }

        [Fact]
        public async Task InterruptAsync_LogsInformation()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.InterruptAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SkipAsync_CallsCorrectEndpoint()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync("/sdapi/v1/skip", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SkipAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/skip", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task SkipAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();

            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SkipAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(x => x.PostAsync("/sdapi/v1/skip", cts.Token), Times.Once);
        }

        [Fact]
        public async Task SkipAsync_LogsInformation()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SkipAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetProgressAsync_WithCompleteState_ReturnsAllFields()
        {
            // Arrange
            var progress = new GenerationProgress
            {
                Progress = 1.0,
                EtaRelative = 0.0,
                State = new ProgressState
                {
                    Skipped = false,
                    Interrupted = false,
                    Job = "txt2img",
                    JobCount = 1,
                    JobNo = 0,
                    SamplingStep = 20,
                    SamplingSteps = 20,
                },
                CurrentImage = "base64preview",
                TextInfo = "Complete",
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<GenerationProgress>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(progress);

            // Act
            var result = await _service.GetProgressAsync();

            // Assert
            result.Progress.Should().Be(1.0);
            result.State!.Skipped.Should().BeFalse();
            result.State.Interrupted.Should().BeFalse();
            result.TextInfo.Should().Be("Complete");
        }
    }
}
