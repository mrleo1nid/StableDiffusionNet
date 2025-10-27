using StableDiffusionNet.Helpers;
using StableDiffusionNet.Interfaces;

namespace StableDiffusionNet.Models
{
    /// <summary>
    /// Контейнер для всех сервисов Stable Diffusion Client.
    /// Применяет Parameter Object Pattern для упрощения конструктора.
    /// </summary>
    public class StableDiffusionServices
    {
        /// <summary>
        /// Сервис генерации изображений из текста
        /// </summary>
        public ITextToImageService TextToImage { get; set; } = null!;

        /// <summary>
        /// Сервис генерации изображений из изображений
        /// </summary>
        public IImageToImageService ImageToImage { get; set; } = null!;

        /// <summary>
        /// Дополнительный сервис (upscaling, face restoration)
        /// </summary>
        public IExtraService Extra { get; set; } = null!;

        /// <summary>
        /// Сервис управления моделями
        /// </summary>
        public IModelService Models { get; set; } = null!;

        /// <summary>
        /// Сервис отслеживания прогресса генерации
        /// </summary>
        public IProgressService Progress { get; set; } = null!;

        /// <summary>
        /// Сервис управления опциями WebUI
        /// </summary>
        public IOptionsService Options { get; set; } = null!;

        /// <summary>
        /// Сервис управления сэмплерами
        /// </summary>
        public ISamplerService Samplers { get; set; } = null!;

        /// <summary>
        /// Сервис управления планировщиками (schedulers)
        /// </summary>
        public ISchedulerService Schedulers { get; set; } = null!;

        /// <summary>
        /// Сервис управления апскейлерами
        /// </summary>
        public IUpscalerService Upscalers { get; set; } = null!;

        /// <summary>
        /// Сервис получения информации из PNG метаданных
        /// </summary>
        public IPngInfoService PngInfo { get; set; } = null!;

        /// <summary>
        /// Сервис управления эмбеддингами (textual inversion)
        /// </summary>
        public IEmbeddingService Embeddings { get; set; } = null!;

        /// <summary>
        /// Сервис управления LoRA моделями
        /// </summary>
        public ILoraService Loras { get; set; } = null!;

        /// <summary>
        /// Валидирует что все обязательные сервисы инициализированы
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Если какой-либо сервис не инициализирован</exception>
        public void Validate()
        {
            Guard.ThrowIfNull(TextToImage);
            Guard.ThrowIfNull(ImageToImage);
            Guard.ThrowIfNull(Extra);
            Guard.ThrowIfNull(Models);
            Guard.ThrowIfNull(Progress);
            Guard.ThrowIfNull(Options);
            Guard.ThrowIfNull(Samplers);
            Guard.ThrowIfNull(Schedulers);
            Guard.ThrowIfNull(Upscalers);
            Guard.ThrowIfNull(PngInfo);
            Guard.ThrowIfNull(Embeddings);
            Guard.ThrowIfNull(Loras);
        }
    }
}
