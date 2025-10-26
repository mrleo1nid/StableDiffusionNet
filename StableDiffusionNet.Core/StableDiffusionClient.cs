using System;
using System.Threading;
using System.Threading.Tasks;
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
            TextToImage =
                textToImageService ?? throw new ArgumentNullException(nameof(textToImageService));
            ImageToImage =
                imageToImageService ?? throw new ArgumentNullException(nameof(imageToImageService));
            Models = modelService ?? throw new ArgumentNullException(nameof(modelService));
            Progress = progressService ?? throw new ArgumentNullException(nameof(progressService));
            Options = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
            Samplers = samplerService ?? throw new ArgumentNullException(nameof(samplerService));
            Schedulers =
                schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            Upscalers = upscalerService ?? throw new ArgumentNullException(nameof(upscalerService));
            PngInfo = pngInfoService ?? throw new ArgumentNullException(nameof(pngInfoService));
            Extra = extraService ?? throw new ArgumentNullException(nameof(extraService));
            Embeddings =
                embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            Loras = loraService ?? throw new ArgumentNullException(nameof(loraService));
            _httpClientWrapper =
                httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
