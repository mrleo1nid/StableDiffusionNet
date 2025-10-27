using System;
using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Главный интерфейс клиента Stable Diffusion WebUI API
    /// </summary>
    public interface IStableDiffusionClient : IDisposable
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            , IAsyncDisposable
#endif
    {
        /// <summary>
        /// Сервис для генерации изображений из текста
        /// </summary>
        ITextToImageService TextToImage { get; }

        /// <summary>
        /// Сервис для генерации изображений из изображений
        /// </summary>
        IImageToImageService ImageToImage { get; }

        /// <summary>
        /// Сервис для работы с моделями
        /// </summary>
        IModelService Models { get; }

        /// <summary>
        /// Сервис для получения прогресса генерации
        /// </summary>
        IProgressService Progress { get; }

        /// <summary>
        /// Сервис для работы с опциями
        /// </summary>
        IOptionsService Options { get; }

        /// <summary>
        /// Сервис для работы с sampler'ами
        /// </summary>
        ISamplerService Samplers { get; }

        /// <summary>
        /// Сервис для работы с scheduler'ами (планировщиками)
        /// </summary>
        ISchedulerService Schedulers { get; }

        /// <summary>
        /// Сервис для работы с апскейлерами
        /// </summary>
        IUpscalerService Upscalers { get; }

        /// <summary>
        /// Сервис для извлечения PNG метаданных
        /// </summary>
        IPngInfoService PngInfo { get; }

        /// <summary>
        /// Сервис для постобработки изображений
        /// </summary>
        IExtraService Extra { get; }

        /// <summary>
        /// Сервис для работы с embeddings
        /// </summary>
        IEmbeddingService Embeddings { get; }

        /// <summary>
        /// Сервис для работы с LoRA моделями
        /// </summary>
        ILoraService Loras { get; }

        /// <summary>
        /// Выполняет детальную проверку доступности API (health check).
        /// Возвращает информацию о состоянии API, времени ответа и возможных ошибках.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Объект с детальной информацией о состоянии API</returns>
        /// <example>
        /// Проверка доступности API:
        /// <code>
        /// var client = new StableDiffusionClientBuilder()
        ///     .WithBaseUrl("http://localhost:7860")
        ///     .Build();
        ///
        /// var healthCheck = await client.HealthCheckAsync();
        ///
        /// if (healthCheck.IsHealthy)
        /// {
        ///     Console.WriteLine($"API доступен. Время ответа: {healthCheck.ResponseTime?.TotalMilliseconds}ms");
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"API недоступен: {healthCheck.Error}");
        /// }
        /// </code>
        /// </example>
        Task<HealthCheckResult> HealthCheckAsync(CancellationToken cancellationToken = default);
    }
}
