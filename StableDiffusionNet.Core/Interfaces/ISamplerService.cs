using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для получения информации о sampler'ах
    /// Single Responsibility Principle - отвечает только за операции с sampler'ами
    /// </summary>
    public interface ISamplerService
    {
        /// <summary>
        /// Получает список доступных sampler'ов с полной информацией
        /// </summary>
        Task<IReadOnlyList<Sampler>> GetSamplersAsync(
            CancellationToken cancellationToken = default
        );
    }
}
