using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet
{
    /// <summary>
    /// Главный клиент для работы с Stable Diffusion WebUI API
    /// Open/Closed Principle - открыт для расширения через интерфейсы сервисов
    /// Dependency Inversion Principle - зависит от абстракций, а не от конкретных реализаций
    /// </summary>
    public class StableDiffusionClient : IStableDiffusionClient
    {
        private readonly ILogger<StableDiffusionClient> _logger;

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

        /// <summary>
        /// Создает экземпляр клиента с внедренными зависимостями
        /// </summary>
        public StableDiffusionClient(
            ITextToImageService textToImageService,
            IImageToImageService imageToImageService,
            IModelService modelService,
            IProgressService progressService,
            IOptionsService optionsService,
            ISamplerService samplerService,
            ILogger<StableDiffusionClient> logger
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
    }
}
