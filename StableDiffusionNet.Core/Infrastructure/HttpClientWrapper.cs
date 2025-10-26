using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Exceptions;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Обертка над HttpClient с обработкой ошибок, логированием и retry логикой
    /// Dependency Inversion Principle - реализует интерфейс IHttpClientWrapper
    /// Single Responsibility - отвечает только за HTTP коммуникацию
    ///
    /// Примечание: Класс не реализует IDisposable, так как HttpClient управляется
    /// извне и не должен диспозиться вручную.
    /// </summary>
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly IStableDiffusionLogger _logger;
        private readonly StableDiffusionOptions _options;
        private readonly RetryHandler _retryHandler;
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// Создает новый экземпляр HTTP клиента
        /// </summary>
        public HttpClientWrapper(
            HttpClient httpClient,
            IStableDiffusionLogger logger,
            StableDiffusionOptions options
        )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _options.Validate();
            _retryHandler = new RetryHandler(_options, _logger);
        }

        /// <inheritdoc/>
        public async Task<TResponse> GetAsync<TResponse>(
            string endpoint,
            CancellationToken cancellationToken = default
        )
            where TResponse : class
        {
            try
            {
                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug($"GET request to {endpoint}");
                }

                var response = await _retryHandler
                    .ExecuteWithRetryAsync(
                        () => _httpClient.GetAsync(endpoint, cancellationToken),
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                return await HandleResponseAsync<TResponse>(response, endpoint, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, $"Error executing GET request to {endpoint}");
                throw new ApiException($"Error executing GET request to {endpoint}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TResponse> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            CancellationToken cancellationToken = default
        )
            where TResponse : class
        {
            try
            {
                var json = JsonConvert.SerializeObject(request, JsonSettings);

                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug(
                        $"POST request to {endpoint} with body: {SanitizeJsonForLogging(json)}"
                    );
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryHandler
                    .ExecuteWithRetryAsync(
                        () => _httpClient.PostAsync(endpoint, content, cancellationToken),
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                return await HandleResponseAsync<TResponse>(response, endpoint, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, $"Error executing POST request to {endpoint}");
                throw new ApiException($"Error executing POST request to {endpoint}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task PostAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug($"POST request to {endpoint} without body");
                }

                var response = await _retryHandler
                    .ExecuteWithRetryAsync(
                        () => _httpClient.PostAsync(endpoint, null, cancellationToken),
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                await EnsureSuccessAsync(response, endpoint, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, $"Error executing POST request to {endpoint}");
                throw new ApiException($"Error executing POST request to {endpoint}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task PostAsync<TRequest>(
            string endpoint,
            TRequest request,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var json = JsonConvert.SerializeObject(request, JsonSettings);

                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug(
                        $"POST request to {endpoint} with body: {SanitizeJsonForLogging(json)}"
                    );
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryHandler
                    .ExecuteWithRetryAsync(
                        () => _httpClient.PostAsync(endpoint, content, cancellationToken),
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                await EnsureSuccessAsync(response, endpoint, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, $"Error executing POST request to {endpoint}");
                throw new ApiException($"Error executing POST request to {endpoint}", ex);
            }
        }

        private async Task<TResponse> HandleResponseAsync<TResponse>(
            HttpResponseMessage response,
            string endpoint,
            CancellationToken cancellationToken
        )
            where TResponse : class
        {
            // В .NET Standard 2.0 и 2.1 ReadAsStringAsync не поддерживает CancellationToken
#if NETSTANDARD2_0 || NETSTANDARD2_1
            _ = cancellationToken; // Подавляем предупреждение о неиспользуемом параметре
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
            var content = await response
                .Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
#endif

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    $"Unsuccessful response from {endpoint}. Status: {response.StatusCode}, Body: {content}"
                );

                throw new ApiException(
                    $"API returned error for {endpoint}",
                    response.StatusCode,
                    content
                );
            }

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug(
                    $"Successful response from {endpoint}: {SanitizeJsonForLogging(content)}"
                );
            }

            try
            {
                var result = JsonConvert.DeserializeObject<TResponse>(content);
                if (result == null)
                {
                    throw new ApiException($"Failed to deserialize response from {endpoint}");
                }
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error deserializing response from {endpoint}");
                throw new ApiException($"Error deserializing response from {endpoint}", ex);
            }
        }

        /// <summary>
        /// Очищает JSON для безопасного логирования, обрезая base64 данные и большие строки
        /// </summary>
        private string SanitizeJsonForLogging(string json)
        {
            const int maxLength = 500;

            if (json.Length <= maxLength)
                return json;

            // Проверяем, содержит ли JSON base64 данные (длинные строки без пробелов)
            if (
                json.Contains("\"data:image")
                || json.Contains("\"init_images\"")
                || json.Contains("\"mask\"")
            )
            {
                return $"[Request with image data, length: {json.Length} chars]";
            }

            // Обрезаем длинные JSON
#if NETSTANDARD2_0
            return json.Substring(0, maxLength)
                + $"... [truncated, total length: {json.Length} chars]";
#else
            return json[..maxLength] + $"... [truncated, total length: {json.Length} chars]";
#endif
        }

        private async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            string endpoint,
            CancellationToken cancellationToken
        )
        {
            if (!response.IsSuccessStatusCode)
            {
                // В .NET Standard 2.0 и 2.1 ReadAsStringAsync не поддерживает CancellationToken
#if NETSTANDARD2_0 || NETSTANDARD2_1
                _ = cancellationToken; // Подавляем предупреждение о неиспользуемом параметре
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
                var content = await response
                    .Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);
#endif
                _logger.LogError(
                    $"Unsuccessful response from {endpoint}. Status: {response.StatusCode}, Body: {content}"
                );

                throw new ApiException(
                    $"API returned error for {endpoint}",
                    response.StatusCode,
                    content
                );
            }
        }
    }
}
