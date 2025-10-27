using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Helpers;
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
            Guard.ThrowIfNull(httpClient);
            Guard.ThrowIfNull(logger);

            _httpClient = httpClient;
            _logger = logger;
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
                    ModelName = u["model_name"]?.ToString(),
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
        public async Task<IReadOnlyList<LatentUpscaleMode>> GetLatentUpscaleModesAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of latent upscale modes");

            var modes = await _httpClient.GetAsync<List<LatentUpscaleMode>>(
                ApiEndpoints.LatentUpscaleModes,
                cancellationToken
            );

            _logger.LogInformation($"Latent upscale modes retrieved: {modes.Count}");

            return modes.AsReadOnly();
        }
    }
}
