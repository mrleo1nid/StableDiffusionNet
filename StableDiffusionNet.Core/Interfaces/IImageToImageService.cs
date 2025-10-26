using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для генерации изображений из изображений (img2img)
    /// Single Responsibility Principle - отвечает только за img2img операции
    /// </summary>
    public interface IImageToImageService
    {
        /// <summary>
        /// Генерирует изображение на основе существующего изображения
        /// </summary>
        /// <param name="request">Параметры генерации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат генерации с изображениями в base64</returns>
        Task<ImageToImageResponse> GenerateAsync(
            ImageToImageRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
