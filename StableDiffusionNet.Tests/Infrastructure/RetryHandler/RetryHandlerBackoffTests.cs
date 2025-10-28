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
    /// Тесты exponential backoff для RetryHandler
    /// </summary>
    public class RetryHandlerBackoffTests
    {
        private readonly Mock<IStableDiffusionLogger> _loggerMock;
        private readonly StableDiffusionOptions _options;

        public RetryHandlerBackoffTests()
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

        #region Exponential Backoff Tests

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
            // Используем широкие диапазоны для учета вариаций на CI/CD серверах
            delays[0].TotalMilliseconds.Should().BeInRange(50, 300); // 100 + большой запас для CI/CD
            delays[1].TotalMilliseconds.Should().BeInRange(100, 500); // 200 + большой запас для CI/CD
            delays[2].TotalMilliseconds.Should().BeInRange(200, 800); // 400 + большой запас для CI/CD

            // Главное - проверяем экспоненциальный рост: каждая задержка больше предыдущей
            delays[1].Should().BeGreaterThan(delays[0]);
            delays[2].Should().BeGreaterThan(delays[1]);
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
            var handler = new RetryHandlerClass(customOptions, _loggerMock.Object);
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
            // Используем широкий диапазон для учета вариаций на CI/CD серверах
            observedDelay!.Value.TotalMilliseconds.Should().BeInRange(400, 900);
        }

        #endregion

        #region Rate Limiting (429) Tests

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
            // Используем большую погрешность для учета вариаций на CI/CD серверах
            observedDelay!
                .Value.TotalMilliseconds.Should()
                .BeGreaterOrEqualTo(_options.RetryDelayMilliseconds * 2 - 50); // большая погрешность для CI/CD
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_RateLimitError429_LogsWithStatusCode()
        {
            // Arrange
            var handler = CreateHandler();
            var callCount = 0;

            Func<Task<HttpResponseMessage>> operation = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return Task.FromResult(new HttpResponseMessage((HttpStatusCode)429));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await handler.ExecuteWithRetryAsync(operation, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.Is<string>(s => s.Contains("[429 TooManyRequests]"))
                    ),
                Times.Once,
                "should log warning with status code"
            );
        }

        #endregion

        #region Jitter Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_MultipleRetriesWithSameError_HasJitterVariation()
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

                if (callCount <= 2)
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
            delays.Should().HaveCount(2);

            // Проверяем, что задержки имеют некоторую вариацию из-за jitter
            // Обе задержки должны быть в разумных пределах для базовой задержки 100ms
            delays[0].TotalMilliseconds.Should().BeInRange(50, 300);
            delays[1].TotalMilliseconds.Should().BeInRange(100, 500);

            // Проверяем, что вторая задержка больше первой (экспоненциальный рост)
            delays[1].Should().BeGreaterThan(delays[0]);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ExecuteWithRetryAsync_SingleRetryWithCustomDelay_RespectsCustomDelay()
        {
            // Arrange
            var customOptions = new StableDiffusionOptions
            {
                BaseUrl = "http://localhost:7860",
                RetryCount = 1,
                RetryDelayMilliseconds = 250,
            };
            var handler = new RetryHandlerClass(customOptions, _loggerMock.Object);
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
            // delay = 250 * (2^0) + jitter = 250 + jitter (0-62.5)
            observedDelay!.Value.TotalMilliseconds.Should().BeInRange(200, 400);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_MaxRetriesWithExponentialBackoff_LogsAllAttempts()
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
            callCount.Should().Be(_options.RetryCount + 1);

            // Проверяем, что логировались все попытки с правильными номерами
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
    }
}
