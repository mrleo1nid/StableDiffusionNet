using StableDiffusionNet.Configuration;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Services;

namespace StableDiffusionNet.Infrastructure
{
    /// <summary>
    /// Фабрика по умолчанию для создания стандартных реализаций сервисов Stable Diffusion
    /// </summary>
    internal class DefaultServiceFactory : IStableDiffusionServiceFactory
    {
        /// <inheritdoc/>
        public ITextToImageService CreateTextToImageService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new TextToImageService(httpClient, logger, validationOptions);
        }

        /// <inheritdoc/>
        public IImageToImageService CreateImageToImageService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new ImageToImageService(httpClient, logger, validationOptions);
        }

        /// <inheritdoc/>
        public IModelService CreateModelService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new ModelService(httpClient, logger);
        }

        /// <inheritdoc/>
        public IProgressService CreateProgressService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new ProgressService(httpClient, logger);
        }

        /// <inheritdoc/>
        public IOptionsService CreateOptionsService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new OptionsService(httpClient, logger);
        }

        /// <inheritdoc/>
        public ISamplerService CreateSamplerService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new SamplerService(httpClient, logger);
        }

        /// <inheritdoc/>
        public ISchedulerService CreateSchedulerService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new SchedulerService(httpClient, logger);
        }

        /// <inheritdoc/>
        public IUpscalerService CreateUpscalerService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new UpscalerService(httpClient, logger);
        }

        /// <inheritdoc/>
        public IPngInfoService CreatePngInfoService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new PngInfoService(httpClient, logger);
        }

        /// <inheritdoc/>
        public IExtraService CreateExtraService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new ExtraService(httpClient, logger);
        }

        /// <inheritdoc/>
        public IEmbeddingService CreateEmbeddingService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new EmbeddingService(httpClient, logger);
        }

        /// <inheritdoc/>
        public ILoraService CreateLoraService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        )
        {
            return new LoraService(httpClient, logger);
        }
    }
}
