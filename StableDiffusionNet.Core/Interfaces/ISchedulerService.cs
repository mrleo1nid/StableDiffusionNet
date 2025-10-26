using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для получения информации о scheduler'ах (планировщиках)
    /// Single Responsibility Principle - отвечает только за операции с scheduler'ами
    /// </summary>
    public interface ISchedulerService
    {
        /// <summary>
        /// Получает список доступных scheduler'ов (планировщиков шагов)
        /// </summary>
        Task<IReadOnlyList<string>> GetSchedulersAsync(
            CancellationToken cancellationToken = default
        );
    }
}
