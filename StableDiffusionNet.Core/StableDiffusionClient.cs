using System;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models;

namespace StableDiffusionNet
{
    /// <summary>
    /// Главный клиент для работы с Stable Diffusion WebUI API
    /// Open/Closed Principle - открыт для расширения через интерфейсы сервисов
    /// Dependency Inversion Principle - зависит от абстракций, а не от конкретных реализаций
    /// </summary>
    public class StableDiffusionClient : IStableDiffusionClient
    {
        private readonly IStableDiffusionLogger _logger;
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly IDisposable? _additionalDisposable;
        private bool _disposed;

        /// <inheritdoc/>
        public ITextToImageService TextToImage { get; }

        /// <inheritdoc/>
        public IImageToImageService ImageToImage { get; }

        /// <inheritdoc/>
        public IModelService Models { get; }

        /// <inheritdoc/>
        public IProgressService Progress { get; }

        /// <inheritdoc/>
        public IOptionsService Options { get; }

        /// <inheritdoc/>
        public ISamplerService Samplers { get; }

        /// <inheritdoc/>
        public ISchedulerService Schedulers { get; }

        /// <inheritdoc/>
        public IUpscalerService Upscalers { get; }

        /// <inheritdoc/>
        public IPngInfoService PngInfo { get; }

        /// <inheritdoc/>
        public IExtraService Extra { get; }

        /// <inheritdoc/>
        public IEmbeddingService Embeddings { get; }

        /// <inheritdoc/>
        public ILoraService Loras { get; }

        /// <summary>
        /// Создает экземпляр клиента с внедренными зависимостями (рекомендуемый конструктор)
        /// Использует Parameter Object Pattern для упрощения управления зависимостями
        /// </summary>
        /// <param name="services">Контейнер со всеми сервисами Stable Diffusion</param>
        /// <param name="httpClientWrapper">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="additionalDisposable">Дополнительный ресурс для освобождения (например, HttpClient созданный в Builder)</param>
        /// <example>
        /// <code>
        /// var services = new StableDiffusionServices
        /// {
        ///     TextToImage = textToImageService,
        ///     ImageToImage = imageToImageService,
        ///     // ... остальные сервисы
        /// };
        /// var client = new StableDiffusionClient(services, httpClient, logger);
        /// </code>
        /// </example>
        public StableDiffusionClient(
            StableDiffusionServices services,
            IHttpClientWrapper httpClientWrapper,
            IStableDiffusionLogger logger,
            IDisposable? additionalDisposable = null
        )
        {
            Guard.ThrowIfNull(services, nameof(services));
            Guard.ThrowIfNull(httpClientWrapper, nameof(httpClientWrapper));
            Guard.ThrowIfNull(logger, nameof(logger));

            // Валидируем что все сервисы инициализированы
            services.Validate();

            TextToImage = services.TextToImage;
            ImageToImage = services.ImageToImage;
            Models = services.Models;
            Progress = services.Progress;
            Options = services.Options;
            Samplers = services.Samplers;
            Schedulers = services.Schedulers;
            Upscalers = services.Upscalers;
            PngInfo = services.PngInfo;
            Extra = services.Extra;
            Embeddings = services.Embeddings;
            Loras = services.Loras;
            _httpClientWrapper = httpClientWrapper;
            _logger = logger;
            _additionalDisposable = additionalDisposable;
        }

        /// <summary>
        /// Выполняет детальную проверку доступности API (health check).
        /// Возвращает информацию о состоянии API, времени ответа и возможных ошибках.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Объект с детальной информацией о состоянии API</returns>
        public async Task<HealthCheckResult> HealthCheckAsync(
            CancellationToken cancellationToken = default
        )
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var endpoint = "/internal/ping"; // Легкий endpoint для проверки

            try
            {
                _logger.LogDebug("Performing API health check");

                // Используем легкий GET запрос вместо полноценного API вызова
                await _httpClientWrapper.GetAsync<object>(endpoint, cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation(
                    $"API is healthy. Response time: {stopwatch.ElapsedMilliseconds}ms"
                );

                return HealthCheckResult.Success(stopwatch.Elapsed, endpoint);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex, "API health check failed");

                return HealthCheckResult.Failure(ex.Message, endpoint);
            }
        }

        /// <summary>
        /// Освобождает ресурсы, используемые клиентом
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Освобождает управляемые и неуправляемые ресурсы
        /// </summary>
        /// <param name="disposing">True если вызван из Dispose, false из финализатора</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Освобождаем HttpClientWrapper
                _httpClientWrapper?.Dispose();

                // Освобождаем дополнительные ресурсы (например, HttpClient созданный в Builder)
                _additionalDisposable?.Dispose();
            }

            _disposed = true;
        }

#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
        /// <summary>
        /// Асинхронно освобождает ресурсы, используемые клиентом
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            // Освобождаем HttpClientWrapper
            _httpClientWrapper?.Dispose();

            // Освобождаем дополнительные ресурсы
            _additionalDisposable?.Dispose();

            _disposed = true;

            // Подавляем вызов финализатора
            GC.SuppressFinalize(this);

            // Добавляем await для возможности будущих асинхронных операций
            await Task.CompletedTask.ConfigureAwait(false);
        }
#endif
    }
}
