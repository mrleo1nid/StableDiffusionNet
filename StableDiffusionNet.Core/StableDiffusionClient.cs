using System;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;

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
        /// Создает экземпляр клиента с внедренными зависимостями
        /// </summary>
#pragma warning disable CA1508, S107 // Избегайте неиспользуемого условного кода
        public StableDiffusionClient(
#pragma warning restore CA1508, S107
            ITextToImageService textToImageService,
            IImageToImageService imageToImageService,
            IModelService modelService,
            IProgressService progressService,
            IOptionsService optionsService,
            ISamplerService samplerService,
            ISchedulerService schedulerService,
            IUpscalerService upscalerService,
            IPngInfoService pngInfoService,
            IExtraService extraService,
            IEmbeddingService embeddingService,
            ILoraService loraService,
            IHttpClientWrapper httpClientWrapper,
            IStableDiffusionLogger logger
        )
        {
            Guard.ThrowIfNull(textToImageService);
            Guard.ThrowIfNull(imageToImageService);
            Guard.ThrowIfNull(modelService);
            Guard.ThrowIfNull(progressService);
            Guard.ThrowIfNull(optionsService);
            Guard.ThrowIfNull(samplerService);
            Guard.ThrowIfNull(schedulerService);
            Guard.ThrowIfNull(upscalerService);
            Guard.ThrowIfNull(pngInfoService);
            Guard.ThrowIfNull(extraService);
            Guard.ThrowIfNull(embeddingService);
            Guard.ThrowIfNull(loraService);
            Guard.ThrowIfNull(httpClientWrapper);
            Guard.ThrowIfNull(logger);

            TextToImage = textToImageService;
            ImageToImage = imageToImageService;
            Models = modelService;
            Progress = progressService;
            Options = optionsService;
            Samplers = samplerService;
            Schedulers = schedulerService;
            Upscalers = upscalerService;
            PngInfo = pngInfoService;
            Extra = extraService;
            Embeddings = embeddingService;
            Loras = loraService;
            _httpClientWrapper = httpClientWrapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Checking API availability");

                // Используем эндпоинт /sdapi/v1/samplers как простую проверку
                await Samplers.GetSamplersAsync(cancellationToken);

                _logger.LogInformation("API is available");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API is unavailable");
                return false;
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
                // Освобождаем HttpClientWrapper, который освободит HttpClient если владеет им
                _httpClientWrapper?.Dispose();
            }

            _disposed = true;
        }
    }
}
