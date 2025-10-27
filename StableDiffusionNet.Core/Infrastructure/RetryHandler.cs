using System;
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
    /// Handler для реализации retry логики с экспоненциальной задержкой
    /// </summary>
    internal class RetryHandler
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
        private static readonly ThreadLocal<Random> _threadLocalRandom = new ThreadLocal<Random>(
            () =>
                new Random(Guid.NewGuid().GetHashCode())
        );
#endif

        public RetryHandler(StableDiffusionOptions options, IStableDiffusionLogger logger)
        {
            Guard.ThrowIfNull(options);
            Guard.ThrowIfNull(logger);

            _options = options;
            _logger = logger;
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
        private static bool IsTransientError(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.RequestTimeout
                || // 408
                statusCode == (HttpStatusCode)429
                || // Too Many Requests
                statusCode == HttpStatusCode.InternalServerError
                || // 500
                statusCode == HttpStatusCode.BadGateway
                || // 502
                statusCode == HttpStatusCode.ServiceUnavailable
                || // 503
                statusCode == HttpStatusCode.GatewayTimeout; // 504
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
#if NET6_0_OR_GREATER
            var jitter = Random.Shared.Next(0, baseDelay / 4);
#else
            var jitter = _threadLocalRandom.Value!.Next(0, baseDelay / 4);
#endif
            return delay + jitter;
        }
    }
}
