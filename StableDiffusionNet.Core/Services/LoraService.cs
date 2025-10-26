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
    /// Сервис для работы с LoRA моделями
    /// Single Responsibility Principle - отвечает только за операции с LoRA
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class LoraService : ILoraService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса работы с LoRA моделями
        /// </summary>
        public LoraService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Lora>> GetLorasAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available LoRA models");

            var lorasArray = await _httpClient.GetAsync<JArray>(
                ApiEndpoints.Loras,
                cancellationToken
            );

            var loras = lorasArray
                .Select(l => new Lora
                {
                    Name = l["name"]?.ToString() ?? string.Empty,
                    Alias = l["alias"]?.ToString(),
                    Path = l["path"]?.ToString(),
                    Metadata = l["metadata"],
                })
                .Where(l => !string.IsNullOrEmpty(l.Name))
                .ToList();

            _logger.LogInformation($"LoRA models retrieved: {loras.Count}");

            return loras.AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task RefreshLorasAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Refreshing LoRA models list");

            await _httpClient.PostAsync(ApiEndpoints.RefreshLoras, cancellationToken);

            _logger.LogInformation("LoRA models refreshed successfully");
        }
    }
}
