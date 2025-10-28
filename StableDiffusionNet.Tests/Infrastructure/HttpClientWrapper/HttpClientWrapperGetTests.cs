using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Exceptions;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests.Infrastructure.HttpClientWrapperTests
{
    /// <summary>
    /// Тесты для HttpClientWrapper - методы GetAsync
    /// </summary>
    public class HttpClientWrapperGetTests
    {
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly StableDiffusionOptions _options;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public HttpClientWrapperGetTests()
        {
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _options = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                TimeoutSeconds = 300,
                EnableDetailedLogging = false,
            };
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        private HttpClientWrapper CreateWrapper()
        {
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                // HttpClient должен быть настроен (обычно это делается в ServiceCollectionExtensions)
                BaseAddress = new Uri(_options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            };
            return new HttpClientWrapper(httpClient, _loggerMock.Object, _options);
        }

        [Fact]
        public async Task GetAsync_SuccessfulResponse_ReturnsDeserializedObject()
        {
            // Arrange
            var expectedResponse = new TestResponse { Id = 1, Name = "Test" };
            var responseJson = JsonConvert.SerializeObject(expectedResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseJson),
                    }
                );

            var wrapper = CreateWrapper();

            // Act
            var result = await wrapper.GetAsync<TestResponse>("/test");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Test");
        }

        [Fact]
        public async Task GetAsync_ErrorResponse_ThrowsApiException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("Bad Request"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test");
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAsync_InvalidJson_ThrowsApiException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("invalid json"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test");
            await act.Should().ThrowAsync<ApiException>();
        }

        [Fact]
        public async Task GetAsync_WithCancellation_ThrowsApiException()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new OperationCanceledException());

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test", cts.Token);
            await act.Should().ThrowAsync<ApiException>();
        }

        [Fact]
        public async Task GetAsync_WithDetailedLogging_LogsRequestAndResponse()
        {
            // Arrange
            var expectedResponse = new TestResponse { Id = 1, Name = "Test" };
            var responseJson = JsonConvert.SerializeObject(expectedResponse);

            _options.EnableDetailedLogging = true;

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseJson),
                    }
                );

            var wrapper = CreateWrapper();

            // Act
            await wrapper.GetAsync<TestResponse>("/test");

            // Assert
            _loggerMock.Verify(
                x =>
                    x.Log(
                        It.IsAny<LogLevel>(),
                        It.Is<string>(s => s.Contains("GET") && s.Contains("/test"))
                    ),
                Times.AtLeastOnce
            );
        }

        #region Additional GetAsync Tests

        [Fact]
        public async Task GetAsync_NullResponseAfterDeserialization_ThrowsApiException()
        {
            // Arrange - возвращаем "null" как строку
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("null"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test");
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.Message.Contains("Failed to deserialize response from /test"));
        }

        [Fact]
        public async Task GetAsync_EmptyResponse_ThrowsApiException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(""),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test");
            await act.Should().ThrowAsync<ApiException>();
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetAsync_WithDifferentErrorCodes_ThrowsApiExceptionWithCorrectCode(
            HttpStatusCode statusCode
        )
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = new StringContent($"Error {statusCode}"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test");
            await act.Should().ThrowAsync<ApiException>().Where(e => e.StatusCode == statusCode);
        }

        [Fact]
        public async Task GetAsync_ErrorResponse_IncludesResponseBodyInException()
        {
            // Arrange
            var errorBody = "Detailed error message";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent(errorBody),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test");
            await act.Should().ThrowAsync<ApiException>().Where(e => e.ResponseBody == errorBody);
        }

        [Fact]
        public async Task GetAsync_WithException_LogsError()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var wrapper = CreateWrapper();

            // Act
            try
            {
                await wrapper.GetAsync<TestResponse>("/test");
            }
            catch
            {
                // Expected
            }

            // Assert
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<Exception>(),
                        It.Is<string>(s => s.Contains("Error executing GET request"))
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_ErrorResponse_LogsErrorWithDetails()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("Bad Request"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act
            try
            {
                await wrapper.GetAsync<TestResponse>("/test");
            }
            catch
            {
                // Expected
            }

            // Assert
            _loggerMock.Verify(
                x => x.Log(LogLevel.Error, It.Is<string>(s => s.Contains("Unsuccessful response"))),
                Times.Once
            );
        }

        #endregion
    }

    // Test classes
    public class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
