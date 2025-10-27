using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для генерации изображений из текста (txt2img)
    /// Single Responsibility Principle - отвечает только за txt2img операции
    /// </summary>
    public interface ITextToImageService
    {
        /// <summary>
        /// Генерирует изображение из текстового описания
        /// </summary>
        /// <param name="request">Параметры генерации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат генерации с изображениями в base64</returns>
        /// <example>
        /// Базовая генерация изображения:
        /// <code>
        /// var client = new StableDiffusionClientBuilder()
        ///     .WithBaseUrl("http://localhost:7860")
        ///     .Build();
        ///
        /// var request = new TextToImageRequest
        /// {
        ///     Prompt = "a beautiful landscape with mountains and lake, sunset, high quality",
        ///     NegativePrompt = "blurry, low quality, watermark",
        ///     Width = 512,
        ///     Height = 512,
        ///     Steps = 20,
        ///     CfgScale = 7,
        ///     BatchSize = 1
        /// };
        ///
        /// var response = await client.TextToImage.GenerateAsync(request);
        ///
        /// // Сохранение изображений
        /// var imageHelper = new ImageHelper();
        /// for (int i = 0; i &lt; response.Images.Count; i++)
        /// {
        ///     await imageHelper.Base64ToImageAsync(
        ///         response.Images[i],
        ///         $"output_{i}.png"
        ///     );
        /// }
        /// </code>
        ///
        /// С дополнительными параметрами:
        /// <code>
        /// var request = new TextToImageRequest
        /// {
        ///     Prompt = "cyberpunk city, neon lights, futuristic",
        ///     Width = 768,
        ///     Height = 768,
        ///     Steps = 30,
        ///     CfgScale = 8.5,
        ///     Seed = 42, // Фиксированный seed для воспроизводимости
        ///     SamplerName = "DPM++ 2M Karras",
        ///     BatchSize = 4 // Генерация 4 изображений за раз
        /// };
        ///
        /// var response = await client.TextToImage.GenerateAsync(request);
        /// Console.WriteLine($"Сгенерировано {response.Images.Count} изображений");
        /// </code>
        /// </example>
        Task<TextToImageResponse> GenerateAsync(
            TextToImageRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
