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
                return await HandleResponseAsync<TResponse>(response, endpoint);
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
                        json
                    );
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                return await HandleResponseAsync<TResponse>(response, endpoint);
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
                await EnsureSuccessAsync(response, endpoint);
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
                        json
                    );
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                await EnsureSuccessAsync(response, endpoint);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error executing POST request to {Endpoint}", endpoint);
                throw new ApiException($"Error executing POST request to {endpoint}", ex);
            }
        }

        private async Task<TResponse> HandleResponseAsync<TResponse>(
            HttpResponseMessage response,
            string endpoint
        )
            where TResponse : class
        {
            var content = await response.Content.ReadAsStringAsync();

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
                _logger.LogDebug("Successful response from {Endpoint}: {Body}", endpoint, content);
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

        private async Task EnsureSuccessAsync(HttpResponseMessage response, string endpoint)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
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
