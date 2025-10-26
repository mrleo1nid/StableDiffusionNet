using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Interfaces;
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
        private readonly ILogger<ModelService> _logger;
        private const string ModelsEndpoint = "/sdapi/v1/sd-models";
        private const string OptionsEndpoint = "/sdapi/v1/options";
        private const string RefreshEndpoint = "/sdapi/v1/refresh-checkpoints";

        /// <summary>
        /// Создает новый экземпляр сервиса управления моделями
        /// </summary>
        public ModelService(IHttpClientWrapper httpClient, ILogger<ModelService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SdModel>> GetModelsAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available models");

            var models = await _httpClient.GetAsync<List<SdModel>>(
                ModelsEndpoint,
                cancellationToken
            );

            _logger.LogInformation("Models retrieved: {Count}", models.Count);

            return models.AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<string> GetCurrentModelAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting current active model");

            var options = await _httpClient.GetAsync<WebUIOptions>(
                OptionsEndpoint,
                cancellationToken
            );

            var currentModel = options.SdModelCheckpoint ?? "unknown";
            _logger.LogInformation("Current model: {Model}", currentModel);

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

            _logger.LogInformation("Setting model: {Model}", modelName);

            var options = new WebUIOptions { SdModelCheckpoint = modelName };

            await _httpClient.PostAsync(OptionsEndpoint, options, cancellationToken);

            _logger.LogInformation("Model successfully set: {Model}", modelName);
        }

        /// <inheritdoc/>
        public async Task RefreshModelsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Refreshing models list");

            await _httpClient.PostAsync(RefreshEndpoint, cancellationToken);

            _logger.LogInformation("Models list successfully refreshed");
        }
    }
}
