using StableDiffusionNet.Configuration;
using StableDiffusionNet.Logging;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Фабрика для создания сервисов Stable Diffusion
    /// Следует Dependency Inversion Principle - позволяет заменять реализации сервисов
    /// </summary>
    public interface IStableDiffusionServiceFactory
    {
        /// <summary>
        /// Создает сервис генерации изображений из текста
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр ITextToImageService</returns>
        ITextToImageService CreateTextToImageService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис генерации изображений из изображений
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IImageToImageService</returns>
        IImageToImageService CreateImageToImageService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис управления моделями
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IModelService</returns>
        IModelService CreateModelService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис отслеживания прогресса
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IProgressService</returns>
        IProgressService CreateProgressService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис управления опциями
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IOptionsService</returns>
        IOptionsService CreateOptionsService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис управления сэмплерами
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр ISamplerService</returns>
        ISamplerService CreateSamplerService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис управления планировщиками
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр ISchedulerService</returns>
        ISchedulerService CreateSchedulerService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис управления апскейлерами
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IUpscalerService</returns>
        IUpscalerService CreateUpscalerService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис получения информации из PNG
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IPngInfoService</returns>
        IPngInfoService CreatePngInfoService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает дополнительный сервис
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IExtraService</returns>
        IExtraService CreateExtraService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис управления эмбеддингами
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр IEmbeddingService</returns>
        IEmbeddingService CreateEmbeddingService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );

        /// <summary>
        /// Создает сервис управления LoRA
        /// </summary>
        /// <param name="httpClient">HTTP клиент wrapper</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validationOptions">Опции валидации</param>
        /// <returns>Экземпляр ILoraService</returns>
        ILoraService CreateLoraService(
            IHttpClientWrapper httpClient,
            IStableDiffusionLogger logger,
            ValidationOptions validationOptions
        );
    }
}
