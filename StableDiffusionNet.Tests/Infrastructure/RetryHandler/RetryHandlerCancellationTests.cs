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
    /// Тесты отмены для RetryHandler
    /// </summary>
    public class RetryHandlerCancellationTests
    {
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly StableDiffusionOptions _options;

        public RetryHandlerCancellationTests()
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

        #region Cancellation Token Tests

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

            Func<Task<HttpResponseMessage>> operation = async () =>
            {
                callCount++;
                if (callCount == 2)
                {
                    await cts.CancelAsync(); // Отменяем во время retry
                }
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            };

            // Act
            var act = async () => await handler.ExecuteWithRetryAsync(operation, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_TaskCanceledWithCancellationToken_DoesNotRetry()
        {
            // Arrange
            var handler = CreateHandler();
            using var cts = new CancellationTokenSource();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = async () =>
            {
                callCount++;
                await cts.CancelAsync();
                throw new TaskCanceledException("Cancelled");
            };

            // Act
            var act = async () => await handler.ExecuteWithRetryAsync(operation, cts.Token);

            // Assert
            await act.Should().ThrowAsync<TaskCanceledException>();
            callCount.Should().Be(1, "should not retry when cancellation token is cancelled");
        }

        #endregion

        #region TaskCanceledException Tests

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
        public async Task ExecuteWithRetryAsync_TaskCanceledExceptionMaxRetries_ThrowsException()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                // Симулируем timeout (TaskCanceledException без отмены токена)
                throw new TaskCanceledException("Request timed out");
            };

            // Act
            var act = async () =>
                await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TaskCanceledException>().WithMessage("Request timed out");
            callCount.Should().Be(_options.RetryCount + 1);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("timed out"))),
                Times.Exactly(_options.RetryCount),
                "should log warning for each retry attempt"
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_TaskCanceledExceptionAfterMaxRetries_ThrowsOriginalException()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;
            var expectedException = new TaskCanceledException("Request timed out");

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                throw expectedException;
            };

            // Act
            var act = async () =>
                await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<TaskCanceledException>();
            exception.Which.Should().BeSameAs(expectedException);
            callCount.Should().Be(_options.RetryCount + 1);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.Is<string>(s => s.Contains("timed out"))),
                Times.Exactly(_options.RetryCount),
                "should log warning for each retry attempt"
            );
        }

        #endregion

        #region Dispose Tests (.NET Standard 2.x only)

#if !NET6_0_OR_GREATER
        [Fact]
        public void Dispose_CanBeCalledSuccessfully()
        {
            // Arrange
            var handler = CreateHandler();

            // Act & Assert - должен вызваться без исключений
            Action act = () => handler.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_MultipleCalls_IsIdempotent()
        {
            // Arrange
            var handler = CreateHandler();

            // Act - вызываем Dispose несколько раз
#pragma warning disable IDE0068, CA1816, CA2000, CA2202, S3966
            Action act = () =>
            {
                handler.Dispose();
                handler.Dispose();
                handler.Dispose();
            };
#pragma warning restore IDE0068, CA1816, CA2000, CA2202, S3966

            // Assert - не должно быть исключений при множественных вызовах
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Dispose_AfterUsage_WorksCorrectly()
        {
            // Arrange
            var handler = CreateHandler();
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            Func<Task<HttpResponseMessage>> operation = () => Task.FromResult(response);

            // Act - используем handler, затем освобождаем
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            Action disposeAct = () => handler.Dispose();

            // Assert
            result.Should().Be(response);
            disposeAct.Should().NotThrow();
        }
#endif

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ExecuteWithRetryAsync_CancellationDuringDelay_ThrowsOperationCanceledException()
        {
            // Arrange
            var handler = CreateHandler();
            using var cts = new CancellationTokenSource();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // Запускаем отмену через небольшую задержку, чтобы она произошла во время retry delay
                    Task.Run(async () =>
                    {
                        await Task.Delay(50); // Небольшая задержка
                        await cts.CancelAsync();
                    });
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    );
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var act = async () => await handler.ExecuteWithRetryAsync(operation, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_NoCancellationToken_WorksNormally()
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
        }

        #endregion
    }
}
