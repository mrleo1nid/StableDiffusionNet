using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для извлечения метаданных из PNG изображений
    /// Single Responsibility Principle - отвечает только за операции с PNG метаданными
    /// </summary>
    public interface IPngInfoService
    {
        /// <summary>
        /// Извлекает метаданные генерации из PNG изображения
        /// </summary>
        Task<PngInfoResponse> GetPngInfoAsync(
            PngInfoRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
