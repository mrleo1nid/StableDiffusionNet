using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для генерации изображений из текста (txt2img)
    /// Single Responsibility Principle - отвечает только за txt2img операции
    /// </summary>
    public interface ITextToImageService
    {
        /// <summary>
        /// Генерирует изображение из текстового описания
        /// </summary>
        /// <param name="request">Параметры генерации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат генерации с изображениями в base64</returns>
        Task<TextToImageResponse> GenerateAsync(
            TextToImageRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
