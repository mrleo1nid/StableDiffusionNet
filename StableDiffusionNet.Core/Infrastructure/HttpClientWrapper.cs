using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Exceptions;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Обертка над HttpClient с обработкой ошибок, логированием и retry логикой
    /// Dependency Inversion Principle - реализует интерфейс IHttpClientWrapper
    /// Single Responsibility - отвечает только за HTTP коммуникацию
    /// </summary>
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly IStableDiffusionLogger _logger;
        private readonly StableDiffusionOptions _options;
        private readonly RetryHandler _retryHandler;
        private readonly JsonSanitizer _jsonSanitizer;
        private readonly bool _ownsHttpClient;
        private bool _disposed;

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// Создает новый экземпляр HTTP клиента с явным контролем владения ресурсами
        /// </summary>
        /// <param name="httpClient">HttpClient для использования</param>
        /// <param name="logger">Логгер</param>
        /// <param name="options">Опции конфигурации</param>
        /// <param name="ownsHttpClient">
        /// True если wrapper должен освободить HttpClient при Dispose.
        /// False если HttpClient управляется извне (например, IHttpClientFactory).
        /// По умолчанию false для безопасности - предотвращает случайное двойное освобождение.
        /// </param>
        public HttpClientWrapper(
            HttpClient httpClient,
            IStableDiffusionLogger logger,
            StableDiffusionOptions options,
            bool ownsHttpClient = false
        )
        {
            Guard.ThrowIfNull(httpClient);
            Guard.ThrowIfNull(logger);
            Guard.ThrowIfNull(options);

            _httpClient = httpClient;
            _logger = logger;
            _options = options;
            _ownsHttpClient = ownsHttpClient;

            _options.Validate();
            _retryHandler = new RetryHandler(_options, _logger);
            _jsonSanitizer = new JsonSanitizer(_options.Validation);
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
                        $"POST request to {endpoint} with body: {_jsonSanitizer.SanitizeForLogging(json)}"
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
                        $"POST request to {endpoint} with body: {_jsonSanitizer.SanitizeForLogging(json)}"
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
                    $"Successful response from {endpoint}: {_jsonSanitizer.SanitizeForLogging(content)}"
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

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Освобождает управляемые и неуправляемые ресурсы
        /// </summary>
        /// <param name="disposing">True если вызван из Dispose, false из финализатора</param>
        /// <remarks>
        /// HttpClient освобождается только если ownsHttpClient = true (указано в конструкторе).
        /// RetryHandler освобождается для предотвращения утечки памяти ThreadLocal в .NET Standard 2.0/2.1.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Освобождаем HttpClient только если мы его владельцы
                // В Builder сценарии (создали сами): ownsHttpClient = true → освобождаем
                // В DI сценарии (IHttpClientFactory): ownsHttpClient = false → не трогаем
                // В Builder сценарии (передан извне): ownsHttpClient = false → не трогаем
                if (_ownsHttpClient)
                    _httpClient?.Dispose();

#if !NET6_0_OR_GREATER
                // Освобождаем RetryHandler для предотвращения утечки памяти ThreadLocal<Random>
                (_retryHandler as IDisposable)?.Dispose();
#endif
            }

            _disposed = true;
        }
    }
}
