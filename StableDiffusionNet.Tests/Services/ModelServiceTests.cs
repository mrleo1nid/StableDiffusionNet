using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Тесты для ModelService
    /// </summary>
    public class ModelServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly ModelService _service;

        public ModelServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new ModelService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ModelService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new ModelService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetModelsAsync_ReturnsListOfModels()
        {
            // Arrange
            var expectedModels = new List<SdModel>
            {
                new SdModel { Title = "Model 1", ModelName = "model1.safetensors" },
                new SdModel { Title = "Model 2", ModelName = "model2.ckpt" },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<SdModel>>("/sdapi/v1/sd-models", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(expectedModels);

            // Act
            var result = await _service.GetModelsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Title.Should().Be("Model 1");
            result[1].Title.Should().Be("Model 2");
        }

        [Fact]
        public async Task GetModelsAsync_ReturnsReadOnlyList()
        {
            // Arrange
            var models = new List<SdModel> { new SdModel { Title = "Model 1" } };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<SdModel>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(models);

            // Act
            var result = await _service.GetModelsAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<SdModel>>();
        }

        [Fact]
        public async Task GetModelsAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var models = new List<SdModel>();

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<SdModel>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(models);

            // Act
            await _service.GetModelsAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<List<SdModel>>("/sdapi/v1/sd-models", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetCurrentModelAsync_ReturnsModelName()
        {
            // Arrange
            var options = new WebUIOptions { SdModelCheckpoint = "current-model.safetensors" };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<WebUIOptions>("/sdapi/v1/options", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(options);

            // Act
            var result = await _service.GetCurrentModelAsync();

            // Assert
            result.Should().Be("current-model.safetensors");
        }

        [Fact]
        public async Task GetCurrentModelAsync_WithNullCheckpoint_ReturnsUnknown()
        {
            // Arrange
            var options = new WebUIOptions { SdModelCheckpoint = null };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<WebUIOptions>("/sdapi/v1/options", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(options);

            // Act
            var result = await _service.GetCurrentModelAsync();

            // Assert
            result.Should().Be("unknown");
        }

        [Fact]
        public async Task GetCurrentModelAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var options = new WebUIOptions { SdModelCheckpoint = "model" };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<WebUIOptions>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(options);

            // Act
            await _service.GetCurrentModelAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<WebUIOptions>("/sdapi/v1/options", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task SetModelAsync_WithValidName_CallsHttpClient()
        {
            // Arrange
            var modelName = "new-model.safetensors";

            _httpClientMock
                .Setup(x =>
                    x.PostAsync(
                        "/sdapi/v1/options",
                        It.IsAny<WebUIOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            // Act
            await _service.SetModelAsync(modelName);

            // Assert
            _httpClientMock.Verify(
                x =>
                    x.PostAsync(
                        "/sdapi/v1/options",
                        It.Is<WebUIOptions>(o => o.SdModelCheckpoint == modelName),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task SetModelAsync_WithEmptyName_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _service.SetModelAsync(string.Empty);
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*Model name cannot be empty*")
                .WithParameterName("modelName");
        }

        [Fact]
        public async Task SetModelAsync_WithWhitespaceName_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _service.SetModelAsync("   ");
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("modelName");
        }

        [Fact]
        public async Task SetModelAsync_WithNullName_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _service.SetModelAsync(null!);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SetModelAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            var modelName = "model.safetensors";
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
            await _service.SetModelAsync(modelName, cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/options", It.IsAny<WebUIOptions>(), cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshModelsAsync_CallsCorrectEndpoint()
        {
            // Arrange
            _httpClientMock
                .Setup(x =>
                    x.PostAsync("/sdapi/v1/refresh-checkpoints", It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshModelsAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/refresh-checkpoints", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshModelsAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();

            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshModelsAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/refresh-checkpoints", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetModelsAsync_LogsInformation()
        {
            // Arrange
            var models = new List<SdModel> { new SdModel { Title = "Model" } };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<List<SdModel>>(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(models);

            // Act
            await _service.GetModelsAsync();

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SetModelAsync_LogsInformation()
        {
            // Arrange
            var modelName = "test-model";

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
            await _service.SetModelAsync(modelName);

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
