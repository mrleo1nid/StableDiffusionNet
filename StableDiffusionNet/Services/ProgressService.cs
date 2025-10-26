using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private const string ProgressEndpoint = "/sdapi/v1/progress";
        private const string InterruptEndpoint = "/sdapi/v1/interrupt";
        private const string SkipEndpoint = "/sdapi/v1/skip";

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
                ProgressEndpoint,
                cancellationToken
            );

            if (progress.State != null)
            {
                _logger.LogDebug(
                    "Прогресс: {Progress:P}, Шаг: {CurrentStep}/{TotalSteps}",
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
            _logger.LogInformation("Прерывание текущей генерации");

            await _httpClient.PostAsync(InterruptEndpoint, cancellationToken);

            _logger.LogInformation("Генерация успешно прервана");
        }

        /// <inheritdoc/>
        public async Task SkipAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Пропуск текущего изображения");

            await _httpClient.PostAsync(SkipEndpoint, cancellationToken);

            _logger.LogInformation("Изображение успешно пропущено");
        }
    }
}
