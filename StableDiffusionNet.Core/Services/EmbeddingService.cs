using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Сервис для работы с embeddings (textual inversions)
    /// Single Responsibility Principle - отвечает только за операции с embeddings
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class EmbeddingService : IEmbeddingService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса работы с embeddings
        /// </summary>
        public EmbeddingService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyDictionary<string, Embedding>> GetEmbeddingsAsync(
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogDebug("Getting list of available embeddings");

            var response = await _httpClient.GetAsync<JObject>(
                ApiEndpoints.Embeddings,
                cancellationToken
            );

            var loaded = response["loaded"] as JObject;
            if (loaded == null)
            {
                _logger.LogInformation("No embeddings loaded");
                return new ReadOnlyDictionary<string, Embedding>(
                    new Dictionary<string, Embedding>()
                );
            }

            var embeddings = new Dictionary<string, Embedding>();
            foreach (var property in loaded.Properties())
            {
                var embeddingData = property.Value as JObject;
                if (embeddingData != null)
                {
                    embeddings[property.Name] = new Embedding
                    {
                        Name = property.Name,
                        VectorCount = embeddingData["vec"]?.ToObject<int?>(),
                        Step = embeddingData["step"]?.ToObject<int?>(),
                        SdCheckpoint = embeddingData["sd_checkpoint"]?.ToString(),
                        SdCheckpointName = embeddingData["sd_checkpoint_name"]?.ToString(),
                    };
                }
            }

            _logger.LogInformation($"Embeddings retrieved: {embeddings.Count}");

            return new ReadOnlyDictionary<string, Embedding>(embeddings);
        }

        /// <inheritdoc/>
        public async Task RefreshEmbeddingsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Refreshing embeddings list");

            await _httpClient.PostAsync(ApiEndpoints.RefreshEmbeddings, cancellationToken);

            _logger.LogInformation("Embeddings refreshed successfully");
        }
    }
}
