using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Exceptions;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests.Infrastructure
{
    /// <summary>
    /// Тесты для HttpClientWrapper
    /// </summary>
    public class HttpClientWrapperTests
    {
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly StableDiffusionOptions _options;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public HttpClientWrapperTests()
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
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Arrange
            // Act & Assert
            var act = () => new HttpClientWrapper(null!, _loggerMock.Object, _options);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();
            // Act & Assert
            var act = () => new HttpClientWrapper(httpClient, null!, _options);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            var act = () => new HttpClientWrapper(httpClient, _loggerMock.Object, null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithInvalidOptions_ThrowsConfigurationException()
        {
            // Act & Assert - валидация теперь происходит в setter
            var act = () => new StableDiffusionOptions { BaseUrl = string.Empty };
            act.Should().Throw<ArgumentException>().WithMessage("*BaseUrl cannot be empty*");
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
        public async Task PostAsync_WithRequestAndResponse_ReturnsDeserializedObject()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
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
            var result = await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Test");
        }

        [Fact]
        public async Task PostAsync_WithoutBody_CompletesSuccessfully()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.PostAsync("/test");
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostAsync_WithRequestNoResponse_CompletesSuccessfully()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.PostAsync("/test", request);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostAsync_ErrorResponse_ThrowsApiException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

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
                        StatusCode = HttpStatusCode.InternalServerError,
                        Content = new StringContent("Server Error"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () =>
                await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
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
                .ThrowsAsync(new TaskCanceledException());

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.GetAsync<TestResponse>("/test", cts.Token);
            await act.Should().ThrowAsync<ApiException>();
        }

        [Fact]
        public async Task GetAsync_WithDetailedLogging_LogsRequestAndResponse()
        {
            // Arrange
            _options.EnableDetailedLogging = true;
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
            await wrapper.GetAsync<TestResponse>("/test");

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Debug, It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log when detailed logging is enabled"
            );
        }

        // Тест удален: Authorization header теперь устанавливается в ServiceCollectionExtensions,
        // а не в HttpClientWrapper. HttpClient приходит уже настроенным.

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
                .WithMessage("*Failed to deserialize response*");
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
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
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
                        Content = new StringContent("Error"),
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
            var errorBody = "Detailed error message from API";
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
                // Ignore
            }

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<Exception>(), It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log error when exception occurs"
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
                        StatusCode = HttpStatusCode.InternalServerError,
                        Content = new StringContent("Server error details"),
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
                // Ignore
            }

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log error with details when error response occurs"
            );
        }

        #endregion

        #region Additional PostAsync Tests

        [Fact]
        public async Task PostAsync_WithRequestAndResponse_InvalidJson_ThrowsApiException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

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
            var act = async () =>
                await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);
            await act.Should().ThrowAsync<ApiException>();
        }

        [Fact]
        public async Task PostAsync_WithRequestAndResponse_NullResponse_ThrowsApiException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

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
            var act = async () =>
                await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);
            await act.Should()
                .ThrowAsync<ApiException>()
                .WithMessage("*Failed to deserialize response*");
        }

        [Fact]
        public async Task PostAsync_WithCancellation_ThrowsApiException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException());

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () =>
                await wrapper.PostAsync<TestRequest, TestResponse>("/test", request, cts.Token);
            await act.Should().ThrowAsync<ApiException>();
        }

        [Fact]
        public async Task PostAsync_WithDetailedLogging_LogsRequestAndResponse()
        {
            // Arrange
            _options.EnableDetailedLogging = true;
            var request = new TestRequest { Value = "test" };
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
            await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);

            // Assert - проверяем логирование запроса и ответа
            _loggerMock.Verify(
                l => l.Log(LogLevel.Debug, It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log when detailed logging is enabled"
            );
        }

        [Fact]
        public async Task PostAsync_WithoutBody_ErrorResponse_ThrowsApiException()
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
            var act = async () => await wrapper.PostAsync("/test");
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostAsync_WithRequestNoResponse_ErrorResponse_ThrowsApiException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

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
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent("Not Found"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.PostAsync("/test", request);
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostAsync_WithoutBody_WithHttpRequestException_ThrowsApiException()
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

            // Act & Assert
            var act = async () => await wrapper.PostAsync("/test");
            await act.Should()
                .ThrowAsync<ApiException>()
                .WithMessage("*Error executing POST request*")
                .Where(e => e.InnerException is HttpRequestException);
        }

        [Fact]
        public async Task PostAsync_WithoutBody_WithException_LogsError()
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
                await wrapper.PostAsync("/test");
            }
            catch
            {
                // Ignore
            }

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<Exception>(), It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log error when exception occurs"
            );
        }

        [Fact]
        public async Task PostAsync_WithRequestNoResponse_WithHttpRequestException_ThrowsApiException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.PostAsync("/test", request);
            await act.Should()
                .ThrowAsync<ApiException>()
                .WithMessage("*Error executing POST request*")
                .Where(e => e.InnerException is HttpRequestException);
        }

        [Fact]
        public async Task PostAsync_WithRequestNoResponse_WithException_LogsError()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Connection timeout"));

            var wrapper = CreateWrapper();

            // Act
            try
            {
                await wrapper.PostAsync("/test", request);
            }
            catch
            {
                // Ignore
            }

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<Exception>(), It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log error when exception occurs"
            );
        }

        [Fact]
        public async Task PostAsync_WithoutBody_WithTaskCanceledException_ThrowsApiException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException());

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.PostAsync("/test");
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.InnerException is TaskCanceledException);
        }

        [Fact]
        public async Task PostAsync_WithRequestNoResponse_WithTaskCanceledException_ThrowsApiException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException());

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () => await wrapper.PostAsync("/test", request);
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.InnerException is TaskCanceledException);
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task PostAsync_WithDifferentErrorCodes_ThrowsApiExceptionWithCorrectCode(
            HttpStatusCode statusCode
        )
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

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
                        Content = new StringContent("Error"),
                    }
                );

            var wrapper = CreateWrapper();

            // Act & Assert
            var act = async () =>
                await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);
            await act.Should().ThrowAsync<ApiException>().Where(e => e.StatusCode == statusCode);
        }

        [Fact]
        public async Task PostAsync_ErrorResponse_IncludesResponseBodyInException()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var errorBody = "Detailed error from API";

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
            var act = async () =>
                await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);
            await act.Should().ThrowAsync<ApiException>().Where(e => e.ResponseBody == errorBody);
        }

        [Fact]
        public async Task PostAsync_WithException_LogsError()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };

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
                await wrapper.PostAsync<TestRequest, TestResponse>("/test", request);
            }
            catch
            {
                // Ignore
            }

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<Exception>(), It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log error when exception occurs"
            );
        }

        [Fact]
        public async Task PostAsync_WithoutBody_AndDetailedLogging_LogsRequest()
        {
            // Arrange
            _options.EnableDetailedLogging = true;

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var wrapper = CreateWrapper();

            // Act
            await wrapper.PostAsync("/test");

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Debug, It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log request when detailed logging is enabled"
            );
        }

        [Fact]
        public async Task PostAsync_WithRequestNoResponse_AndDetailedLogging_LogsRequest()
        {
            // Arrange
            _options.EnableDetailedLogging = true;
            var request = new TestRequest { Value = "test" };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var wrapper = CreateWrapper();

            // Act
            await wrapper.PostAsync("/test", request);

            // Assert
            _loggerMock.Verify(
                l => l.Log(LogLevel.Debug, It.IsAny<string>()),
                Times.AtLeastOnce,
                "Should log request when detailed logging is enabled"
            );
        }

        #endregion

        // Вспомогательные классы для тестов
        private class TestRequest
        {
            public string Value { get; set; } = string.Empty;
        }

        private class TestResponse
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
