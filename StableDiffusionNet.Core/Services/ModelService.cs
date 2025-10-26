using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для управления моделями
    /// Single Responsibility Principle - отвечает только за операции с моделями
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class ModelService : IModelService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса управления моделями
        /// </summary>
        public ModelService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            Guard.ThrowIfNull(httpClient);
            Guard.ThrowIfNull(logger);

            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SdModel>> GetModelsAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available models");

            var models = await _httpClient.GetAsync<List<SdModel>>(
                ApiEndpoints.Models,
                cancellationToken
            );

            _logger.LogInformation($"Models retrieved: {models.Count}");

            return models.AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<string> GetCurrentModelAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting current active model");

            var options = await _httpClient.GetAsync<WebUIOptions>(
                ApiEndpoints.Options,
                cancellationToken
            );

            var currentModel = options.SdModelCheckpoint ?? "unknown";
            _logger.LogInformation($"Current model: {currentModel}");

            return currentModel;
        }

        /// <inheritdoc/>
        public async Task SetModelAsync(
            string modelName,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty", nameof(modelName));

            _logger.LogInformation($"Setting model: {modelName}");

            var options = new WebUIOptions { SdModelCheckpoint = modelName };

            await _httpClient.PostAsync(ApiEndpoints.Options, options, cancellationToken);

            _logger.LogInformation($"Model successfully set: {modelName}");
        }

        /// <inheritdoc/>
        public async Task RefreshModelsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Refreshing models list");

            await _httpClient.PostAsync(ApiEndpoints.RefreshCheckpoints, cancellationToken);

            _logger.LogInformation("Models list successfully refreshed");
        }
    }
}
