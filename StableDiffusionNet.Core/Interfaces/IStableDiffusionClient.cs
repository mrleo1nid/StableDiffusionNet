using System.Threading;
using System.Threading.Tasks;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Главный интерфейс клиента Stable Diffusion WebUI API
    /// </summary>
    public interface IStableDiffusionClient
    {
        /// <summary>
        /// Сервис для генерации изображений из текста
        /// </summary>
        ITextToImageService TextToImage { get; }

        /// <summary>
        /// Сервис для генерации изображений из изображений
        /// </summary>
        IImageToImageService ImageToImage { get; }

        /// <summary>
        /// Сервис для работы с моделями
        /// </summary>
        IModelService Models { get; }

        /// <summary>
        /// Сервис для получения прогресса генерации
        /// </summary>
        IProgressService Progress { get; }

        /// <summary>
        /// Сервис для работы с опциями
        /// </summary>
        IOptionsService Options { get; }

        /// <summary>
        /// Сервис для работы с sampler'ами
        /// </summary>
        ISamplerService Samplers { get; }

        /// <summary>
        /// Проверка доступности API
        /// </summary>
        Task<bool> PingAsync(CancellationToken cancellationToken = default);
    }
}
