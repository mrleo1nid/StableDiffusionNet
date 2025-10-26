using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для отслеживания прогресса генерации
    /// Single Responsibility Principle - отвечает только за мониторинг прогресса
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class ProgressService : IProgressService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly ILogger<ProgressService> _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса отслеживания прогресса
        /// </summary>
        public ProgressService(IHttpClientWrapper httpClient, ILogger<ProgressService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<GenerationProgress> GetProgressAsync(
            CancellationToken cancellationToken = default
        )
        {
            var progress = await _httpClient.GetAsync<GenerationProgress>(
                ApiEndpoints.Progress,
                cancellationToken
            );

            if (progress.State != null)
            {
                _logger.LogDebug(
                    "Progress: {Progress:P}, Step: {CurrentStep}/{TotalSteps}",
                    progress.Progress,
                    progress.State.SamplingStep,
                    progress.State.SamplingSteps
                );
            }

            return progress;
        }

        /// <inheritdoc/>
        public async Task InterruptAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Interrupting current generation");

            await _httpClient.PostAsync(ApiEndpoints.Interrupt, cancellationToken);

            _logger.LogInformation("Generation successfully interrupted");
        }

        /// <inheritdoc/>
        public async Task SkipAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Skipping current image");

            await _httpClient.PostAsync(ApiEndpoints.Skip, cancellationToken);

            _logger.LogInformation("Image successfully skipped");
        }
    }
}
