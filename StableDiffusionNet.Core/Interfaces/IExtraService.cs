using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для постобработки изображений (апскейл, face restoration)
    /// Single Responsibility Principle - отвечает только за постобработку изображений
    /// </summary>
    public interface IExtraService
    {
        /// <summary>
        /// Выполняет постобработку одного изображения (апскейл, face restoration)
        /// </summary>
        Task<ExtraSingleImageResponse> ProcessSingleImageAsync(
            ExtraSingleImageRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
