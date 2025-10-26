using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для получения информации об апскейлерах
    /// Single Responsibility Principle - отвечает только за операции с апскейлерами
    /// </summary>
    public interface IUpscalerService
    {
        /// <summary>
        /// Получает список доступных апскейлеров
        /// </summary>
        Task<IReadOnlyList<Upscaler>> GetUpscalersAsync(
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Получает список доступных latent upscale режимов
        /// </summary>
        Task<IReadOnlyList<LatentUpscaleMode>> GetLatentUpscaleModesAsync(
            CancellationToken cancellationToken = default
        );
    }
}
