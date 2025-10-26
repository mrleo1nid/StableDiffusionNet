using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с embeddings (textual inversions)
    /// Single Responsibility Principle - отвечает только за операции с embeddings
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Получает список доступных embeddings
        /// </summary>
        Task<IReadOnlyDictionary<string, Embedding>> GetEmbeddingsAsync(
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Обновляет список embeddings
        /// </summary>
        Task RefreshEmbeddingsAsync(CancellationToken cancellationToken = default);
    }
}
