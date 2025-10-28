using System.Net;
using FluentAssertions;
using Moq;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Logging;
using RetryHandlerClass = StableDiffusionNet.Infrastructure.RetryHandler;

namespace StableDiffusionNet.Tests.Infrastructure.RetryHandler
{
    /// <summary>
    /// Базовые тесты для RetryHandler
    /// </summary>
    public class RetryHandlerBasicTests
    {
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly StableDiffusionOptions _options;

        public RetryHandlerBasicTests()
        {
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _options = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                RetryCount = 3,
                RetryDelayMilliseconds = 100,
            };
        }

        private RetryHandlerClass CreateHandler() =>
            new RetryHandlerClass(_options, _loggerMock.Object);

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new RetryHandlerClass(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new RetryHandlerClass(_options, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        #endregion

        #region Success Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_SuccessfulResponse_ReturnsImmediately()
        {
            // Arrange
            var handler = CreateHandler();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var operationCallCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                operationCallCount++;
                return Task.FromResult(expectedResponse);
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.Should().Be(expectedResponse);
            operationCallCount.Should().Be(1, "operation should be called only once on success");
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<string>()),
                Times.Never,
                "should not log warnings on immediate success"
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_SuccessAfterRetry_LogsSuccessMessage()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    );
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            callCount.Should().Be(2);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.Is<string>(s => s.Contains("succeeded after"))),
                Times.Once,
                "should log success after retry"
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_SuccessWithoutRetry_DoesNotLogSuccessMessage()
        {
            // Arrange
            var handler = CreateHandler();

            Func<Task<HttpResponseMessage>> operation = () =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.Is<string>(s => s.Contains("succeeded after"))),
                Times.Never,
                "should not log success message when no retry was needed"
            );
        }

        #endregion

        #region Transient/Non-Transient Error Tests

        [Theory]
        [InlineData(HttpStatusCode.RequestTimeout)] // 408
        [InlineData((HttpStatusCode)429)] // Too Many Requests
        [InlineData(HttpStatusCode.InternalServerError)] // 500
        [InlineData(HttpStatusCode.BadGateway)] // 502
        [InlineData(HttpStatusCode.ServiceUnavailable)] // 503
        [InlineData(HttpStatusCode.GatewayTimeout)] // 504
        public async Task ExecuteWithRetryAsync_TransientError_RetriesAndEventuallySucceeds(
            HttpStatusCode transientStatusCode
        )
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount <= 2)
                {
                    return Task.FromResult(new HttpResponseMessage(transientStatusCode));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            callCount.Should().Be(3);
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.Is<string>(s => s.Contains($"failed with {transientStatusCode}"))
                    ),
                Times.AtLeast(1),
                "should log retry warnings"
            );
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)] // 404
        [InlineData(HttpStatusCode.BadRequest)] // 400
        [InlineData(HttpStatusCode.Unauthorized)] // 401
        [InlineData(HttpStatusCode.Forbidden)] // 403
        public async Task ExecuteWithRetryAsync_NonTransientError_ReturnsImmediately(
            HttpStatusCode nonTransientStatusCode
        )
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                return Task.FromResult(new HttpResponseMessage(nonTransientStatusCode));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(nonTransientStatusCode);
            callCount.Should().Be(1, "should not retry on non-transient errors");
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<string>()),
                Times.Never,
                "should not log warnings for non-transient errors"
            );
        }

        #endregion

        #region HttpRequestException Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_HttpRequestException_RetriesAndSucceeds()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Network error");
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            callCount.Should().Be(2);
            _loggerMock.Verify(
                x =>
                    x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("HttpRequestException"))),
                Times.Once
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_HttpRequestExceptionMaxRetries_ThrowsException()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                throw new HttpRequestException("Network error");
            };

            // Act
            var act = async () =>
                await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
            callCount.Should().Be(_options.RetryCount + 1);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_HttpRequestExceptionAfterAllRetries_ThrowsOriginalException()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;
            var expectedException = new HttpRequestException("Persistent network error");

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                throw expectedException;
            };

            // Act
            var act = async () =>
                await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<HttpRequestException>();
            exception.Which.Should().BeSameAs(expectedException);
            callCount.Should().Be(_options.RetryCount + 1);
            _loggerMock.Verify(
                x =>
                    x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("HttpRequestException"))),
                Times.Exactly(_options.RetryCount),
                "should log warning for each retry attempt"
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_NetworkErrorWithoutStatusCode_LogsNetworkError()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Connection refused");
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("[Network Error]"))),
                Times.Once,
                "should log warning with [Network Error] indicator"
            );
        }

        #endregion

        #region Max Retries Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_MaxRetriesExceeded_ReturnsLastResponse()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            callCount.Should().Be(_options.RetryCount + 1); // initial + retries
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<string>()),
                Times.Exactly(_options.RetryCount),
                "should log warning for each retry attempt"
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_MultipleRetries_LogsRetryAttempts()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount <= 3)
                {
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    );
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            callCount.Should().Be(4); // initial + 3 retries

            // Проверяем, что логировались попытки с правильными номерами
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("attempt 1/3"))),
                Times.Once
            );
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("attempt 2/3"))),
                Times.Once
            );
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("attempt 3/3"))),
                Times.Once
            );
        }

        #endregion

        #region Zero Retry Count Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_ZeroRetryCount_NoRetries()
        {
            // Arrange
            var optionsNoRetry = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                RetryCount = 0,
                RetryDelayMilliseconds = 100,
            };
            var handler = new RetryHandlerClass(optionsNoRetry, _loggerMock.Object);
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            callCount.Should().Be(1, "should not retry when RetryCount is 0");
            _loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Multiple Error Types Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_MultipleTransientErrorTypes_RetriesCorrectly()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                return callCount switch
                {
                    1 => Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    ),
                    2 => throw new HttpRequestException("Timeout"),
                    3 => Task.FromResult(new HttpResponseMessage((HttpStatusCode)429)),
                    _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
                };
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            callCount.Should().Be(4);
        }

        #endregion
    }
}
