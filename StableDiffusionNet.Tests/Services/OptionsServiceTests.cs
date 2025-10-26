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
    /// Тесты для OptionsService
    /// </summary>
    public class OptionsServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly OptionsService _service;

        public OptionsServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new OptionsService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new OptionsService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new OptionsService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetOptionsAsync_ReturnsWebUIOptions()
        {
            // Arrange
            var expectedOptions = new WebUIOptions
            {
                SdModelCheckpoint = "model.safetensors",
                SdVae = "vae.pt",
                ClipStopAtLastLayers = 2,
                EnableHr = true,
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<WebUIOptions>("/sdapi/v1/options", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(expectedOptions);

            // Act
            var result = await _service.GetOptionsAsync();

            // Assert
            result.Should().NotBeNull();
            result.SdModelCheckpoint.Should().Be("model.safetensors");
            result.SdVae.Should().Be("vae.pt");
            result.ClipStopAtLastLayers.Should().Be(2);
            result.EnableHr.Should().BeTrue();
        }

        [Fact]
        public async Task GetOptionsAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var options = new WebUIOptions();

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<WebUIOptions>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(options);

            // Act
            await _service.GetOptionsAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<WebUIOptions>("/sdapi/v1/options", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetOptionsAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var options = new WebUIOptions();

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<WebUIOptions>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(options);

            // Act
            await _service.GetOptionsAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<WebUIOptions>("/sdapi/v1/options", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task SetOptionsAsync_WithValidOptions_CallsHttpClient()
        {
            // Arrange
            var options = new WebUIOptions
            {
                SdModelCheckpoint = "new-model.safetensors",
                EnableHr = true,
            };

            _httpClientMock
                .Setup(x =>
                    x.PostAsync("/sdapi/v1/options", options, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetOptionsAsync(options);

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/options", options, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task SetOptionsAsync_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = async () => await _service.SetOptionsAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("options");
        }

        [Fact]
        public async Task SetOptionsAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            var options = new WebUIOptions();
            using var cts = new CancellationTokenSource();

            _httpClientMock
                .Setup(x =>
                    x.PostAsync(
                        It.IsAny<string>(),
                        It.IsAny<WebUIOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetOptionsAsync(options, cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/options", options, cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task SetOptionsAsync_LogsInformation()
        {
            // Arrange
            var options = new WebUIOptions();

            _httpClientMock
                .Setup(x =>
                    x.PostAsync(
                        It.IsAny<string>(),
                        It.IsAny<WebUIOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetOptionsAsync(options);

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetOptionsAsync_LogsDebug()
        {
            // Arrange
            var options = new WebUIOptions();

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<WebUIOptions>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(options);

            // Act
            await _service.GetOptionsAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SetOptionsAsync_WithComplexOptions_PreservesAllProperties()
        {
            // Arrange
            var options = new WebUIOptions
            {
                SdModelCheckpoint = "model.safetensors",
                SdVae = "vae.pt",
                ClipStopAtLastLayers = 3,
                EnableHr = true,
                HrUpscaler = "Latent",
                FaceRestorationModel = "CodeFormer",
                SamplesSave = true,
                SamplesFormat = "png",
                OutdirSamples = "/output/path",
                EnableXformers = true,
            };

            WebUIOptions? capturedOptions = null;
            _httpClientMock
                .Setup(x =>
                    x.PostAsync(
                        It.IsAny<string>(),
                        It.IsAny<WebUIOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Callback<string, WebUIOptions, CancellationToken>(
                    (_, opts, _) => capturedOptions = opts
                )
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetOptionsAsync(options);

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions!.SdModelCheckpoint.Should().Be("model.safetensors");
            capturedOptions.SdVae.Should().Be("vae.pt");
            capturedOptions.ClipStopAtLastLayers.Should().Be(3);
            capturedOptions.EnableHr.Should().BeTrue();
            capturedOptions.EnableXformers.Should().BeTrue();
        }
    }
}
