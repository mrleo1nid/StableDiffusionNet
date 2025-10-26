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
    /// Тесты для EmbeddingService
    /// </summary>
    public class EmbeddingServiceTests
    {
        private readonly Mock<IHttpClientWrapper> _httpClientMock;
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly EmbeddingService _service;

        public EmbeddingServiceTests()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _service = new EmbeddingService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new EmbeddingService(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new EmbeddingService(_httpClientMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetEmbeddingsAsync_ReturnsDictionaryOfEmbeddings()
        {
            // Arrange
            var responseJson = new JObject
            {
                ["loaded"] = new JObject
                {
                    ["easy-negative"] = new JObject
                    {
                        ["shape"] = 768,
                        ["vectors"] = 1,
                        ["step"] = 1000,
                        ["sd_checkpoint"] = "abc123",
                        ["sd_checkpoint_name"] = "model_v1",
                    },
                    ["bad-hands"] = new JObject
                    {
                        ["shape"] = 1024,
                        ["vectors"] = 2,
                        ["step"] = 2000,
                        ["sd_checkpoint"] = "def456",
                        ["sd_checkpoint_name"] = "model_v2",
                    },
                },
            };

            _httpClientMock
                .Setup(x =>
                    x.GetAsync<JObject>("/sdapi/v1/embeddings", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(responseJson);

            // Act
            var result = await _service.GetEmbeddingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainKey("easy-negative");
            result.Should().ContainKey("bad-hands");

            var easyNeg = result["easy-negative"];
            easyNeg.Name.Should().Be("easy-negative");
            easyNeg.Shape.Should().Be(768);
            easyNeg.Vectors.Should().Be(1);
            easyNeg.Step.Should().Be(1000);
            easyNeg.SdCheckpoint.Should().Be("abc123");
            easyNeg.SdCheckpointName.Should().Be("model_v1");

            var badHands = result["bad-hands"];
            badHands.Name.Should().Be("bad-hands");
            badHands.Shape.Should().Be(1024);
            badHands.Vectors.Should().Be(2);
            badHands.Step.Should().Be(2000);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_WithNoLoadedEmbeddings_ReturnsEmptyDictionary()
        {
            // Arrange
            var responseJson = new JObject { ["loaded"] = null };

            _httpClientMock
                .Setup(x => x.GetAsync<JObject>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseJson);

            // Act
            var result = await _service.GetEmbeddingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEmbeddingsAsync_WithMissingLoadedProperty_ReturnsEmptyDictionary()
        {
            // Arrange
            var responseJson = new JObject();

            _httpClientMock
                .Setup(x => x.GetAsync<JObject>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseJson);

            // Act
            var result = await _service.GetEmbeddingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEmbeddingsAsync_ReturnsReadOnlyDictionary()
        {
            // Arrange
            var responseJson = new JObject
            {
                ["loaded"] = new JObject { ["test-embedding"] = new JObject { ["vec"] = 512 } },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JObject>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseJson);

            // Act
            var result = await _service.GetEmbeddingsAsync();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyDictionary<string, Embedding>>();
        }

        [Fact]
        public async Task GetEmbeddingsAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var responseJson = new JObject { ["loaded"] = new JObject() };

            _httpClientMock
                .Setup(x => x.GetAsync<JObject>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseJson);

            // Act
            await _service.GetEmbeddingsAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JObject>("/sdapi/v1/embeddings", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetEmbeddingsAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var responseJson = new JObject { ["loaded"] = new JObject() };

            _httpClientMock
                .Setup(x => x.GetAsync<JObject>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseJson);

            // Act
            await _service.GetEmbeddingsAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.GetAsync<JObject>("/sdapi/v1/embeddings", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task GetEmbeddingsAsync_WithNullableFields_HandlesGracefully()
        {
            // Arrange
            var responseJson = new JObject
            {
                ["loaded"] = new JObject
                {
                    ["incomplete-embedding"] = new JObject
                    {
                        ["shape"] = null,
                        ["vectors"] = null,
                        ["step"] = null,
                        ["sd_checkpoint"] = null,
                        ["sd_checkpoint_name"] = null,
                    },
                },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JObject>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseJson);

            // Act
            var result = await _service.GetEmbeddingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKey("incomplete-embedding");
            var embedding = result["incomplete-embedding"];
            embedding.Shape.Should().Be(0); // Default value for missing data
            embedding.Vectors.Should().Be(0); // Default value for missing data
            embedding.Step.Should().BeNull();
            embedding.SdCheckpoint.Should().BeNullOrEmpty();
            embedding.SdCheckpointName.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsInformation()
        {
            // Arrange
            var responseJson = new JObject
            {
                ["loaded"] = new JObject { ["emb1"] = new JObject(), ["emb2"] = new JObject() },
            };

            _httpClientMock
                .Setup(x => x.GetAsync<JObject>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseJson);

            // Act
            await _service.GetEmbeddingsAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.Is<string>(s => s.Contains("Getting list"))),
                Times.Once
            );
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.Is<string>(s => s.Contains("Embeddings retrieved: 2"))
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshEmbeddingsAsync_CallsCorrectEndpoint()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshEmbeddingsAsync();

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/refresh-embeddings", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshEmbeddingsAsync_WithCancellationToken_PassesToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshEmbeddingsAsync(cts.Token);

            // Assert
            _httpClientMock.Verify(
                x => x.PostAsync("/sdapi/v1/refresh-embeddings", cts.Token),
                Times.Once
            );
        }

        [Fact]
        public async Task RefreshEmbeddingsAsync_LogsInformation()
        {
            // Arrange
            _httpClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.RefreshEmbeddingsAsync();

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
    }
}
