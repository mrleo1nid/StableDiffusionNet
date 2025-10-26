using System.Net;
using FluentAssertions;
using Moq;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Infrastructure;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Tests.Infrastructure
{
    /// <summary>
    /// Тесты для RetryHandler
    /// </summary>
    public class RetryHandlerTests
    {
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly StableDiffusionOptions _options;

        public RetryHandlerTests()
        {
            _loggerMock = new Mock<IStableDiffusionLogger>();
            _options = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                RetryCount = 3,
                RetryDelayMilliseconds = 100,
            };
        }

        private RetryHandler CreateHandler() => new RetryHandler(_options, _loggerMock.Object);

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new RetryHandler(null!, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new RetryHandler(_options, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

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
        public async Task ExecuteWithRetryAsync_TaskCanceledExceptionTimeout_Retries()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;
            using var cts = new CancellationTokenSource();

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // Симулируем timeout (TaskCanceledException без отмены токена)
                    throw new TaskCanceledException("Request timed out");
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, cts.Token);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            callCount.Should().Be(2);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("timed out"))),
                Times.Once
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var handler = CreateHandler();
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync(); // Отменяем сразу

            Func<Task<HttpResponseMessage>> operation = () =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var act = async () => await handler.ExecuteWithRetryAsync(operation, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_CancellationDuringRetry_ThrowsOperationCanceledException()
        {
            // Arrange
            var handler = CreateHandler();
            using var cts = new CancellationTokenSource();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 2)
                {
                    cts.Cancel(); // Отменяем во время retry
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            };

            // Act
            var act = async () => await handler.ExecuteWithRetryAsync(operation, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_RateLimitError429_UsesIncreasedDelay()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;
            var delayStart = DateTime.UtcNow;
            TimeSpan? observedDelay = null;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    delayStart = DateTime.UtcNow;
                    return Task.FromResult(new HttpResponseMessage((HttpStatusCode)429));
                }
                else if (callCount == 2)
                {
                    observedDelay = DateTime.UtcNow - delayStart;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            callCount.Should().Be(2);
            // Задержка для 429 должна быть больше базовой (baseDelay * 2 для 429)
            // Для первой попытки: delay = (baseDelay * 2) * (2^0) + jitter = 200 + jitter
            observedDelay.Should().NotBeNull();
            observedDelay!
                .Value.TotalMilliseconds.Should()
                .BeGreaterOrEqualTo(_options.RetryDelayMilliseconds * 2 - 10); // небольшая погрешность
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
            var handler = new RetryHandler(optionsNoRetry, _loggerMock.Object);
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

        [Fact]
        public async Task ExecuteWithRetryAsync_ExponentialBackoff_IncreasesDelay()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;
            var delays = new List<TimeSpan>();
            var lastCallTime = DateTime.UtcNow;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                var now = DateTime.UtcNow;
                if (callCount > 0)
                {
                    delays.Add(now - lastCallTime);
                }
                lastCallTime = now;
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
            delays.Should().HaveCount(3);

            // Проверяем, что каждая следующая задержка больше предыдущей (экспоненциальный рост)
            // delay[0] ≈ 100ms (2^0 = 1)
            // delay[1] ≈ 200ms (2^1 = 2)
            // delay[2] ≈ 400ms (2^2 = 4)
            delays[0].TotalMilliseconds.Should().BeInRange(80, 150); // 100 + jitter
            delays[1].TotalMilliseconds.Should().BeInRange(180, 250); // 200 + jitter
            delays[2].TotalMilliseconds.Should().BeInRange(380, 450); // 400 + jitter
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

        [Fact]
        public async Task ExecuteWithRetryAsync_TaskCanceledWithCancellationToken_DoesNotRetry()
        {
            // Arrange
            var handler = CreateHandler();
            using var cts = new CancellationTokenSource();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                cts.Cancel();
                throw new TaskCanceledException("Cancelled");
            };

            // Act
            var act = async () => await handler.ExecuteWithRetryAsync(operation, cts.Token);

            // Assert
            await act.Should().ThrowAsync<TaskCanceledException>();
            callCount.Should().Be(1, "should not retry when cancellation token is cancelled");
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_CustomRetryDelayMilliseconds_UsesCustomDelay()
        {
            // Arrange
            var customOptions = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                RetryCount = 2,
                RetryDelayMilliseconds = 500, // Кастомная задержка
            };
            var handler = new RetryHandler(customOptions, _loggerMock.Object);
            var callCount = 0;
            var delayStart = DateTime.UtcNow;
            TimeSpan? observedDelay = null;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    delayStart = DateTime.UtcNow;
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    );
                }
                else if (callCount == 2)
                {
                    observedDelay = DateTime.UtcNow - delayStart;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            observedDelay.Should().NotBeNull();
            // Первая попытка retry: delay = 500 * (2^0) + jitter = 500 + jitter (0-125)
            observedDelay!.Value.TotalMilliseconds.Should().BeInRange(480, 650);
        }
    }
}
