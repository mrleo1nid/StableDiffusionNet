using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Configuration;
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

        public RetryHandler(StableDiffusionOptions options, IStableDiffusionLogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            Exception? lastException = null;

            while (attempt <= _options.RetryCount)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var response = await operation().ConfigureAwait(false);

                    // Если ответ успешен или это не транзитная ошибка - возвращаем
                    if (response.IsSuccessStatusCode || !IsTransientError(response.StatusCode))
                    {
                        if (attempt > 0)
                        {
                            _logger.LogInformation(
                                $"Request succeeded after {attempt} retry attempt(s)"
                            );
                        }
                        return response;
                    }

                    // Если это транзитная ошибка и есть еще попытки
                    if (attempt < _options.RetryCount)
                    {
                        var delay = CalculateDelay(attempt, response.StatusCode);
                        _logger.LogWarning(
                            $"Request failed with {response.StatusCode}. Retrying in {delay}ms (attempt {attempt + 1}/{_options.RetryCount})"
                        );

                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        attempt++;
                        continue;
                    }

                    // Исчерпаны все попытки
                    return response;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;

                    if (attempt < _options.RetryCount)
                    {
                        var delay = CalculateDelay(attempt, null);
                        _logger.LogWarning(
                            $"Request failed with HttpRequestException: {ex.Message}. Retrying in {delay}ms (attempt {attempt + 1}/{_options.RetryCount})"
                        );

                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        attempt++;
                        continue;
                    }

                    // Исчерпаны все попытки
                    throw;
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // Таймаут
                    lastException = ex;

                    if (attempt < _options.RetryCount)
                    {
                        var delay = CalculateDelay(attempt, null);
                        _logger.LogWarning(
                            $"Request timed out. Retrying in {delay}ms (attempt {attempt + 1}/{_options.RetryCount})"
                        );

                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        attempt++;
                        continue;
                    }

                    // Исчерпаны все попытки
                    throw;
                }
            }

            // Это не должно произойти, но на всякий случай
            if (lastException != null)
            {
                throw lastException;
            }

            throw new InvalidOperationException("Retry logic failed unexpectedly");
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
            var jitter = new Random().Next(0, baseDelay / 4);
            return delay + jitter;
        }
    }
}
