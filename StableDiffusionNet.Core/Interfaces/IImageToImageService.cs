using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Interfaces
{
    /// <summary>
    /// Интерфейс для генерации изображений из изображений (img2img)
    /// Single Responsibility Principle - отвечает только за img2img операции
    /// </summary>
    public interface IImageToImageService
    {
        /// <summary>
        /// Генерирует изображение на основе существующего изображения
        /// </summary>
        /// <param name="request">Параметры генерации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат генерации с изображениями в base64</returns>
        /// <example>
        /// Базовая генерация img2img:
        /// <code>
        /// var client = new StableDiffusionClientBuilder()
        ///     .WithBaseUrl("http://localhost:7860")
        ///     .Build();
        ///
        /// // Загрузка исходного изображения
        /// var imageHelper = new ImageHelper();
        /// var initImage = await imageHelper.ImageToBase64Async("input.png");
        ///
        /// var request = new ImageToImageRequest
        /// {
        ///     Prompt = "make it look like a watercolor painting",
        ///     NegativePrompt = "photo, realistic",
        ///     InitImages = new List&lt;string&gt; { initImage },
        ///     Width = 512,
        ///     Height = 512,
        ///     DenoisingStrength = 0.75, // Сила изменения: 0.0 (без изменений) - 1.0 (полное изменение)
        ///     Steps = 20,
        ///     CfgScale = 7
        /// };
        ///
        /// var response = await client.ImageToImage.GenerateAsync(request);
        ///
        /// // Сохранение результата
        /// await imageHelper.Base64ToImageAsync(
        ///     response.Images[0],
        ///     "output.png"
        /// );
        /// </code>
        ///
        /// С различными уровнями denoising strength:
        /// <code>
        /// // Слабое изменение (больше похоже на исходник)
        /// var lightRequest = new ImageToImageRequest
        /// {
        ///     Prompt = "add slight glow effect",
        ///     InitImages = new List&lt;string&gt; { initImage },
        ///     DenoisingStrength = 0.3, // Легкие изменения
        ///     Width = 512,
        ///     Height = 512
        /// };
        ///
        /// // Сильное изменение (может значительно отличаться от исходника)
        /// var strongRequest = new ImageToImageRequest
        /// {
        ///     Prompt = "completely repaint as sci-fi scene",
        ///     InitImages = new List&lt;string&gt; { initImage },
        ///     DenoisingStrength = 0.9, // Значительные изменения
        ///     Width = 512,
        ///     Height = 512
        /// };
        /// </code>
        /// </example>
        Task<ImageToImageResponse> GenerateAsync(
            ImageToImageRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
