using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StableDiffusionNet.Configuration;
using StableDiffusionNet.Exceptions;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Обертка над HttpClient с обработкой ошибок и логированием
    /// Dependency Inversion Principle - реализует интерфейс IHttpClientWrapper
    /// Single Responsibility - отвечает только за HTTP коммуникацию
    /// </summary>
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientWrapper> _logger;
        private readonly StableDiffusionOptions _options;

        /// <summary>
        /// Создает новый экземпляр HTTP клиента
        /// </summary>
        public HttpClientWrapper(
            HttpClient httpClient,
            ILogger<HttpClientWrapper> logger,
            IOptions<StableDiffusionOptions> options
        )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _options.Validate();
            // HttpClient уже настроен в ServiceCollectionExtensions
            // (BaseAddress, Timeout, Authorization header)
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
                    _logger.LogDebug("GET request to {Endpoint}", endpoint);
                }

                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                return await HandleResponseAsync<TResponse>(response, endpoint, cancellationToken);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error executing GET request to {Endpoint}", endpoint);
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
                var json = JsonConvert.SerializeObject(
                    request,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                );

                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug(
                        "POST request to {Endpoint} with body: {Body}",
                        endpoint,
                        SanitizeJsonForLogging(json)
                    );
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                return await HandleResponseAsync<TResponse>(response, endpoint, cancellationToken);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error executing POST request to {Endpoint}", endpoint);
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
                    _logger.LogDebug("POST request to {Endpoint} without body", endpoint);
                }

                var response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
                await EnsureSuccessAsync(response, endpoint, cancellationToken);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error executing POST request to {Endpoint}", endpoint);
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
                var json = JsonConvert.SerializeObject(
                    request,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                );

                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug(
                        "POST request to {Endpoint} with body: {Body}",
                        endpoint,
                        SanitizeJsonForLogging(json)
                    );
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                await EnsureSuccessAsync(response, endpoint, cancellationToken);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error executing POST request to {Endpoint}", endpoint);
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
            var content = await response.Content.ReadAsStringAsync();
#else
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Unsuccessful response from {Endpoint}. Status: {StatusCode}, Body: {Body}",
                    endpoint,
                    response.StatusCode,
                    content
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
                    "Successful response from {Endpoint}: {Body}",
                    endpoint,
                    SanitizeJsonForLogging(content)
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
                _logger.LogError(ex, "Error deserializing response from {Endpoint}", endpoint);
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
            return json.Substring(0, maxLength)
                + $"... [truncated, total length: {json.Length} chars]";
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
                var content = await response.Content.ReadAsStringAsync();
#else
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
#endif
                _logger.LogError(
                    "Unsuccessful response from {Endpoint}. Status: {StatusCode}, Body: {Body}",
                    endpoint,
                    response.StatusCode,
                    content
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
