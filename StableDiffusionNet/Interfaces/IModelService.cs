using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для управления моделями Stable Diffusion
    /// Single Responsibility Principle - отвечает только за операции с моделями
    /// </summary>
    public interface IModelService
    {
        /// <summary>
        /// Получает список доступных моделей
        /// </summary>
        Task<IReadOnlyList<SdModel>> GetModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает текущую активную модель
        /// </summary>
        Task<string> GetCurrentModelAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Устанавливает активную модель
        /// </summary>
        /// <param name="modelName">Название модели</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        Task SetModelAsync(string modelName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет список моделей
        /// </summary>
        Task RefreshModelsAsync(CancellationToken cancellationToken = default);
    }
}
