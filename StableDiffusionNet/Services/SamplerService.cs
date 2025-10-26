using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для получения информации о sampler'ах
    /// Single Responsibility Principle - отвечает только за операции с sampler'ами
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class SamplerService : ISamplerService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly ILogger<SamplerService> _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса получения информации о sampler'ах
        /// </summary>
        public SamplerService(IHttpClientWrapper httpClient, ILogger<SamplerService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> GetSamplersAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available samplers");

            var samplers = await _httpClient.GetAsync<JArray>(
                ApiEndpoints.Samplers,
                cancellationToken
            );

            var samplerNames = samplers
                .Select(s => s["name"]?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name!)
                .ToList();

            _logger.LogInformation("Samplers retrieved: {Count}", samplerNames.Count);

            return samplerNames.AsReadOnly();
        }
    }
}
