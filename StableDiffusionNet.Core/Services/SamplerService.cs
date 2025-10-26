using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;

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
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса получения информации о sampler'ах
        /// </summary>
        public SamplerService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Sampler>> GetSamplersAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available samplers");

            var samplers = await _httpClient.GetAsync<List<Sampler>>(
                ApiEndpoints.Samplers,
                cancellationToken
            );

            _logger.LogInformation($"Samplers retrieved: {samplers.Count}");

            return samplers.AsReadOnly();
        }
    }
}
