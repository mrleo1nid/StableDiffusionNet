using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для получения информации о sampler'ах
    /// Single Responsibility Principle - отвечает только за операции с sampler'ами
    /// </summary>
    public interface ISamplerService
    {
        /// <summary>
        /// Получает список доступных sampler'ов
        /// </summary>
        Task<IReadOnlyList<string>> GetSamplersAsync(CancellationToken cancellationToken = default);
    }
}
