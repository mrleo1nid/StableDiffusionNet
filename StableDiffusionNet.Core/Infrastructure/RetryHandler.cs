using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Handler для реализации retry логики с экспоненциальной задержкой.
    /// Реализует IDisposable для корректного освобождения ThreadLocal ресурсов в .NET Standard 2.0/2.1
    /// </summary>
    internal class RetryHandler
#if !NET6_0_OR_GREATER
        : IDisposable
#endif
    {
        private readonly StableDiffusionOptions _options;
        private readonly IStableDiffusionLogger _logger;

#if !NET6_0_OR_GREATER
        // ThreadLocal<Random> для thread-safety в старых версиях .NET
        // SuppressMessage: Random используется только для jitter в retry логике, не для криптографии
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Security",
            "S2245:Make sure that using this pseudorandom number generator is safe here.",
            Justification = "Random используется только для добавления jitter к retry задержкам, не требует криптографической стойкости"
        )]
        private readonly ThreadLocal<Random> _threadLocalRandom;
        private bool _disposed;
#endif

        public RetryHandler(StableDiffusionOptions options, IStableDiffusionLogger logger)
        {
            Guard.ThrowIfNull(options);
            Guard.ThrowIfNull(logger);

            _options = options;
            _logger = logger;

#if !NET6_0_OR_GREATER
            // Создаем ThreadLocal в конструкторе для контроля жизненного цикла
            // Random безопасен здесь: используется только для jitter в retry логике, не для криптографии
#pragma warning disable S2245 // Pseudorandom number generators should not be used for security-sensitive applications
            _threadLocalRandom = new ThreadLocal<Random>(
                () => new Random(Guid.NewGuid().GetHashCode()),
                trackAllValues: false // Не отслеживаем значения для производительности
            );
#pragma warning restore S2245
#endif
        }

        /// <summary>
        /// Выполняет операцию с retry логикой
        /// </summary>
        public async Task<HttpResponseMessage> ExecuteWithRetryAsync(
            Func<Task<HttpResponseMessage>> operation,
            CancellationToken cancellationToken
        )
        {
            int attempt = 0;

            while (attempt <= _options.RetryCount)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var response = await operation().ConfigureAwait(false);
                    var result = await HandleResponseAsync(response, attempt, cancellationToken);

                    if (result.shouldReturn)
                        return response;

                    if (result.shouldRetry)
                        attempt++;
                }
                catch (HttpRequestException ex)
                {
                    if (
                        await TryRetryAsync(
                            attempt,
                            null,
                            $"Request failed with HttpRequestException: {ex.Message}",
                            cancellationToken
                        )
                    )
                    {
                        attempt++;
                        continue;
                    }
                    throw;
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    if (await TryRetryAsync(attempt, null, "Request timed out", cancellationToken))
                    {
                        attempt++;
                        continue;
                    }
                    throw;
                }
            }

            throw new InvalidOperationException("Retry logic failed unexpectedly");
        }

        private async Task<(bool shouldReturn, bool shouldRetry)> HandleResponseAsync(
            HttpResponseMessage response,
            int attempt,
            CancellationToken cancellationToken
        )
        {
            if (response.IsSuccessStatusCode || !IsTransientError(response.StatusCode))
            {
                LogSuccessIfRetried(attempt);
                return (true, false);
            }

            var shouldRetry = await TryRetryAsync(
                attempt,
                response.StatusCode,
                $"Request failed with {response.StatusCode}",
                cancellationToken
            );

            return (!shouldRetry, shouldRetry);
        }

        private void LogSuccessIfRetried(int attempt)
        {
            if (attempt > 0)
            {
                _logger.LogInformation($"Request succeeded after {attempt} retry attempt(s)");
            }
        }

        private async Task<bool> TryRetryAsync(
            int attempt,
            HttpStatusCode? statusCode,
            string message,
            CancellationToken cancellationToken
        )
        {
            if (attempt >= _options.RetryCount)
            {
                return false;
            }

            var delay = CalculateDelay(attempt, statusCode);
            var statusInfo = statusCode.HasValue
                ? $"[{(int)statusCode.Value} {statusCode.Value}]"
                : "[Network Error]";

            _logger.LogWarning(
                $"{statusInfo} {message}. Retrying in {delay}ms (attempt {attempt + 1}/{_options.RetryCount})"
            );
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Определяет, является ли ошибка транзитной (временной)
        /// </summary>
        private static readonly HashSet<HttpStatusCode> TransientStatusCodes =
            new HashSet<HttpStatusCode>
            {
                HttpStatusCode.RequestTimeout, // 408
                (HttpStatusCode)429, // Too Many Requests
                HttpStatusCode.InternalServerError, // 500
                HttpStatusCode.BadGateway, // 502
                HttpStatusCode.ServiceUnavailable, // 503
                HttpStatusCode.GatewayTimeout, // 504
            };

        /// <summary>
        /// Определяет, является ли ошибка транзитной (временной)
        /// </summary>
        private static bool IsTransientError(HttpStatusCode statusCode)
        {
            return TransientStatusCodes.Contains(statusCode);
        }

        /// <summary>
        /// Вычисляет задержку с экспоненциальным backoff
        /// </summary>
        private int CalculateDelay(int attempt, HttpStatusCode? statusCode)
        {
            var baseDelay = _options.RetryDelayMilliseconds;

            // Для rate limiting (429) используем увеличенную задержку
            if (statusCode == (HttpStatusCode)429)
            {
                baseDelay *= 2;
            }

            // Экспоненциальный backoff: delay * (2 ^ attempt)
            var delay = baseDelay * (1 << attempt);

            // Добавляем небольшую случайную задержку (jitter) для избежания thundering herd
            // Random безопасен здесь: используется только для jitter, не для криптографии
#if NET6_0_OR_GREATER
#pragma warning disable S2245 // Pseudorandom number generators should not be used for security-sensitive applications
            var jitter = Random.Shared.Next(0, baseDelay / 4);
#pragma warning restore S2245
#else
            var jitter = _threadLocalRandom.Value!.Next(0, baseDelay / 4);
#endif
            return delay + jitter;
        }

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Освобождает ресурсы, используемые RetryHandler
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Освобождает управляемые ресурсы
        /// </summary>
        /// <param name="disposing">True если вызван из Dispose, false из финализатора</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Освобождаем ThreadLocal<Random> для предотвращения утечки памяти
                _threadLocalRandom?.Dispose();
            }

            _disposed = true;
        }
#endif
    }
}
