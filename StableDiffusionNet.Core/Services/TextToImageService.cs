using System.Threading;
using System.Threading.Tasks;
using StableDiffusionNet.Constants;
using StableDiffusionNet.Helpers;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Logging;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Models.Responses;

namespace StableDiffusionNet.Services
{
    /// <summary>
    /// Сервис для генерации изображений из текста (txt2img)
    /// Single Responsibility Principle - отвечает только за txt2img операции
    /// Dependency Inversion Principle - зависит от абстракции IHttpClientWrapper
    /// </summary>
    public class TextToImageService : ITextToImageService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly IStableDiffusionLogger _logger;

        /// <summary>
        /// Создает новый экземпляр сервиса генерации изображений из текста
        /// </summary>
        public TextToImageService(IHttpClientWrapper httpClient, IStableDiffusionLogger logger)
        {
            Guard.ThrowIfNull(httpClient);
            Guard.ThrowIfNull(logger);

            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TextToImageResponse> GenerateAsync(
            TextToImageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            Guard.ThrowIfNull(request);
            request.Validate(nameof(request));

            _logger.LogInformation(
                $"Starting text-to-image generation. Prompt: {request.Prompt}, Size: {request.Width}x{request.Height}"
            );

            var response = await _httpClient.PostAsync<TextToImageRequest, TextToImageResponse>(
                ApiEndpoints.TextToImage,
                request,
                cancellationToken
            );

            _logger.LogInformation(
                $"Generation completed. Images generated: {response.Images?.Count ?? 0}"
            );

            return response;
        }
    }
}
