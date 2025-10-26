using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для отслеживания прогресса генерации
    /// Single Responsibility Principle - отвечает только за мониторинг прогресса
    /// </summary>
    public interface IProgressService
    {
        /// <summary>
        /// Получает текущий прогресс генерации
        /// </summary>
        Task<GenerationProgress> GetProgressAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Прерывает текущую генерацию
        /// </summary>
        Task InterruptAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Пропускает текущее изображение при батч-генерации
        /// </summary>
        Task SkipAsync(CancellationToken cancellationToken = default);
    }
}
