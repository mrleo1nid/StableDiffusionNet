using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с LoRA моделями
    /// Single Responsibility Principle - отвечает только за операции с LoRA
    /// </summary>
    public interface ILoraService
    {
        /// <summary>
        /// Получает список доступных LoRA моделей
        /// </summary>
        Task<IReadOnlyList<Lora>> GetLorasAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет список LoRA моделей
        /// </summary>
        Task RefreshLorasAsync(CancellationToken cancellationToken = default);
    }
}
