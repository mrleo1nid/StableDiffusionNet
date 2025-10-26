using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для получения информации об апскейлерах
    /// Single Responsibility Principle - отвечает только за операции с апскейлерами
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class UpscalerService : IUpscalerService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса получения информации об апскейлерах
        /// </summary>
        public UpscalerService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Upscaler>> GetUpscalersAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available upscalers");

            var upscalersArray = await _httpClient.GetAsync<JArray>(
                ApiEndpoints.Upscalers,
                cancellationToken
            );

            var upscalers = upscalersArray
                .Select(u => new Upscaler
                {
                    Name = u["name"]?.ToString() ?? string.Empty,
                    ModelPath = u["model_path"]?.ToString(),
                    ModelUrl = u["model_url"]?.ToString(),
                    Scale = u["scale"]?.ToObject<double?>(),
                })
                .Where(u => !string.IsNullOrEmpty(u.Name))
                .ToList();

            _logger.LogInformation($"Upscalers retrieved: {upscalers.Count}");

            return upscalers.AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> GetLatentUpscaleModesAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of latent upscale modes");

            var modesArray = await _httpClient.GetAsync<JArray>(
                ApiEndpoints.LatentUpscaleModes,
                cancellationToken
            );

            var modes = modesArray
                .Select(m => m["name"]?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name!)
                .ToList();

            _logger.LogInformation($"Latent upscale modes retrieved: {modes.Count}");

            return modes.AsReadOnly();
        }
    }
}
