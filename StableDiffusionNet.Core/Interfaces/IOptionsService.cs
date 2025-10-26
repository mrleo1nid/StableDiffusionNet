using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для управления опциями WebUI
    /// Single Responsibility Principle - отвечает только за операции с опциями
    /// </summary>
    public interface IOptionsService
    {
        /// <summary>
        /// Получает текущие опции
        /// </summary>
        Task<WebUIOptions> GetOptionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Устанавливает опции
        /// </summary>
        Task SetOptionsAsync(WebUIOptions options, CancellationToken cancellationToken = default);
    }
}
